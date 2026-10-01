import { test, expect } from "@playwright/test";
import fs from "node:fs";
import path from "node:path";
import { loadEnvConfig } from "../../helpers/env-config";
import { MoveSoakHarness } from "../helpers/harness";
import type { FileBatchSpec, TransferStatusCheckResponse } from "../helpers/types";

// Reproduces the slow NetApp -> Egress transfer of a high-volume small-file
// batch (3,331 small files took 6.5h in the reported incident). Baseline for
// the TransferResultProcessor batching / Egress single-upload work.
//
// Opt-in only (hours long, thousands of files) so `npm run move-soak` skips it:
//   $env:SMALL_FILE_REPRO="1"; npm run small-file-repro
//
// Optional env:
//   SMALL_FILE_REPRO_COUNT               files to seed (default 3331)
//   SMALL_FILE_REPRO_SIZE_KB             size of each file (default 50)
//   SMALL_FILE_REPRO_UPLOAD_CONCURRENCY  parallel Egress uploads while seeding (default 8)
//   SMALL_FILE_REPRO_TARGET_MINUTES      NetApp -> Egress copy must finish within this (default 30)
//   SMALL_FILE_REPRO_EGRESS_PREFIX       reuse files already uploaded to the Egress
//                                        source folder with this name prefix
//   SMALL_FILE_REPRO_SEED_ONLY           seed NetApp and write the manifest, but skip the timed copy
//   SMALL_FILE_REPRO_SEED_MANIFEST     manifest from a previous run - skips
//                                        seeding and re-times the NetApp -> Egress copy
//   SMALL_FILE_REPRO_MIX                 mixed batch instead of COUNT x SIZE_KB, e.g.
//                                        "50KB:100,5MB:10,6MB:10,20MB:10,100MB:2"
//                                        (<=5MB uploads to Egress in one part, >5MB multipart)
//   SMALL_FILE_REPRO_VERIFY_SIZES        "1" to check every copied file's size in Egress
//                                        (on by default for a MIX run)

// Re-running the dev/staging comparison (e.g. once the fix reaches staging):
// seeding is done once; the NetApp seed folders persist (a Copy leaves its
// source), so re-time with SMALL_FILE_REPRO_SEED_MANIFEST pointing at the
// manifests in move-soak/.small-file-repro/ (gitignored, kept locally):
//   dev:     seed-2026-09-29T10-14-07-898Z.json + seed-2026-09-29T10-39-20-941Z.json
//   staging: seed-2026-09-30T14-34-05-879Z.json + seed-2026-09-30T17-19-37-386Z.json
//            + seed-topup-988.json   (run with ENVIRONMENT=staging)
// Both sets total exactly 3,329 x 50KB files. Clear any finished transfer left
// active on the case first, or /initiate is rejected.

const MANIFEST_DIR = path.resolve(__dirname, "../.small-file-repro");

type SeedManifest = {
  netappFolder: string;
  fileNames: string[];
  fileSizeBytes: number;
  // Per-file sizes; absent in manifests written before mixed batches.
  fileSizes?: Record<string, number>;
  seededAt: string;
  seedDurationSeconds: number;
};

// "50KB:100,6MB:10" -> [{ fileSizeMb: 50/1024, fileCount: 100 }, { fileSizeMb: 6, fileCount: 10 }]
function parseMix(mix: string): FileBatchSpec[] {
  return mix.split(",").map((part) => {
    const match = part.trim().match(/^(\d+(?:\.\d+)?)(KB|MB):(\d+)$/i);
    if (!match) throw new Error(`Invalid SMALL_FILE_REPRO_MIX entry "${part}"`);
    const size = Number(match[1]);
    return {
      fileSizeMb: match[2].toUpperCase() === "KB" ? size / 1024 : size,
      fileCount: Number(match[3]),
    };
  });
}

type ProgressSample = { elapsedSeconds: number; processedFiles: number };

async function pollUntilDone(
  harness: MoveSoakHarness,
  transferId: string,
  label: string,
  timeoutMs: number,
): Promise<{ status: TransferStatusCheckResponse | null; samples: ProgressSample[] }> {
  const start = Date.now();
  const samples: ProgressSample[] = [];
  let status: TransferStatusCheckResponse | null = null;

  while (true) {
    status = await harness.checkTransferStatus(transferId);

    const elapsedSeconds = Math.round((Date.now() - start) / 1000);
    const processedFiles = status?.processedFiles ?? 0;
    samples.push({ elapsedSeconds, processedFiles });

    const filesPerMinute = processedFiles / Math.max(elapsedSeconds / 60, 1 / 60);
    if (!status) {
      console.log(`[${label}] transfer ${transferId} not visible yet`);
    } else console.log(
      `[${label}] ${status?.status} ${processedFiles}/${status?.totalFiles} ` +
        `(ok ${status?.successfulFiles}, failed ${status?.failedFiles}) ` +
        `after ${(elapsedSeconds / 60).toFixed(1)} min - ${filesPerMinute.toFixed(1)} files/min`,
    );

    if (
      status?.status === "Completed" ||
      status?.status === "PartiallyCompleted" ||
      status?.status === "Failed"
    ) {
      return { status, samples };
    }

    if (Date.now() - start > timeoutMs) {
      throw new Error(`[${label}] transfer ${transferId} timed out after ${elapsedSeconds}s`);
    }

    await new Promise((r) => setTimeout(r, 60_000));
  }
}

test.describe("NetApp to Egress small-file repro", () => {
  test.skip(
    process.env.SMALL_FILE_REPRO !== "1",
    "Set SMALL_FILE_REPRO=1 to run the multi-hour small-file repro",
  );

  test.setTimeout(14 * 60 * 60 * 1000);

  const config = loadEnvConfig();
  const egressSourceFolder = "1. ABEs for Transcript/";
  const caseId = Number(config.defaultCaseId!);

  const fileCount = Number(process.env.SMALL_FILE_REPRO_COUNT ?? 3331);
  const fileSizeKb = Number(process.env.SMALL_FILE_REPRO_SIZE_KB ?? 50);
  const uploadConcurrency = Number(process.env.SMALL_FILE_REPRO_UPLOAD_CONCURRENCY ?? 8);
  const targetMinutes = Number(process.env.SMALL_FILE_REPRO_TARGET_MINUTES ?? 30);
  const mix = process.env.SMALL_FILE_REPRO_MIX;
  const fileSpecs = mix ? parseMix(mix) : [{ fileSizeMb: fileSizeKb / 1024, fileCount }];
  const verifySizes = process.env.SMALL_FILE_REPRO_VERIFY_SIZES === "1" || !!mix;

  test(`NetApp -> Egress copy of ${mix ?? `${fileCount} x ${fileSizeKb}KB`} files`, async () => {
    const harness = new MoveSoakHarness({
      apiBaseUrl: config.lccApiBaseUrl!,
      egressBaseUrl: config.egressBaseUrl!,
      serviceAccountAuth: config.egressServiceAccountAuth!,
      workspaceId: config.defaultWorkspaceId!,
      egressSourceFolder,
      caseId,
      netappFolderPath: `${config.netAppOperationName!}/`,
      tenantId: config.tenantId!,
      apiClientId: config.lccApiClientId!,
      aadUsername: config.e2eAdUser!,
      aadPassword: config.e2eAdPassword!,
    });

    await harness.setupEgressAuth();
    await harness.setupAadAuth();

    const stamp = new Date().toISOString().replace(/[:.]/g, "-");
    let manifests: SeedManifest[];

    if (process.env.SMALL_FILE_REPRO_SEED_MANIFEST) {
      // Comma-separated, so seeds made in separate runs can be timed as one transfer.
      manifests = process.env.SMALL_FILE_REPRO_SEED_MANIFEST.split(",").map((p) =>
        JSON.parse(fs.readFileSync(p.trim(), "utf8")),
      );
      for (const m of manifests) {
        console.log(`Reusing seed ${m.netappFolder} (${m.fileNames.length} files)`);
      }
    } else {
      // Seed: upload to Egress, then Move into a fresh NetApp subfolder. There
      // is no direct NetApp write path for tests, so this is the only route.
      manifests = [await test.step(`Seed ${mix ?? fileCount} files into NetApp`, async () => {
        const seedStart = Date.now();
        const netappFolder = `${config.netAppOperationName!}/small-file-repro-${stamp}/`;

        const reusePrefix = process.env.SMALL_FILE_REPRO_EGRESS_PREFIX;
        const files = reusePrefix
          ? await harness.listStagedFiles(reusePrefix)
          : await harness.stageFiles(fileSpecs, { concurrency: uploadConcurrency });
        expect(files.length, "no files staged in Egress").toBeGreaterThan(0);
        console.log(
          `Staged ${files.length} files in Egress after ${((Date.now() - seedStart) / 60000).toFixed(1)} min`,
        );

        const move = await harness.startMove(files, netappFolder);
        const { status } = await pollUntilDone(harness, move.id, "seed", 8 * 60 * 60 * 1000);
        await harness.clearTransfer(move.id);

        // A handful of failed moves doesn't invalidate a timing run - seed with
        // whatever landed in NetApp and log the rest.
        expect(status?.successfulFiles, `Seed move ${move.id} moved nothing`).toBeGreaterThan(0);
        const failedNames = new Set(
          (status?.failedItems ?? []).map((p) => p.split("/").pop()),
        );
        if (failedNames.size > 0) {
          console.warn(
            `Seed move ${move.id} ${status?.status}: excluding ${failedNames.size} failed file(s): ` +
              [...failedNames].join(", "),
          );
        }

        const seededFiles = files.filter((f) => !failedNames.has(f.fileName));
        const seeded: SeedManifest = {
          netappFolder,
          fileNames: seededFiles.map((f) => f.fileName),
          fileSizeBytes: files[0].fileSize,
          fileSizes: Object.fromEntries(seededFiles.map((f) => [f.fileName, f.fileSize])),
          seededAt: new Date().toISOString(),
          seedDurationSeconds: Math.round((Date.now() - seedStart) / 1000),
        };

        fs.mkdirSync(MANIFEST_DIR, { recursive: true });
        const manifestPath = path.join(MANIFEST_DIR, `seed-${stamp}.json`);
        fs.writeFileSync(manifestPath, JSON.stringify(seeded, null, 2));
        console.log(`Seed manifest written to ${manifestPath}`);

        return seeded;
      })];
    }

    if (process.env.SMALL_FILE_REPRO_SEED_ONLY === "1") {
      console.log("SMALL_FILE_REPRO_SEED_ONLY set - skipping the timed copy");
      return;
    }

    // Timed NetApp -> Egress copy - the path under investigation.
    const egressDestination = `3. Unused - disclosed/small-file-repro-${stamp}/`;
    const netappPaths = manifests.flatMap((m) =>
      m.fileNames.map((name) => `${m.netappFolder}${name}`),
    );

    const transferStart = Date.now();
    const transfer = await harness.startNetAppToEgressCopy(netappPaths, egressDestination);
    const { status, samples } = await pollUntilDone(
      harness,
      transfer.id,
      "netapp-to-egress",
      12 * 60 * 60 * 1000,
    );
    const durationSeconds = (Date.now() - transferStart) / 1000;

    const sizes = manifests.flatMap((m) =>
      m.fileNames.map((n) => m.fileSizes?.[n] ?? m.fileSizeBytes),
    );
    const countBySize = new Map<number, number>();
    for (const s of sizes) countBySize.set(s, (countBySize.get(s) ?? 0) + 1);
    const sizeBreakdown = [...countBySize]
      .sort(([a], [b]) => a - b)
      .map(([s, n]) => `${n} x ${s >= 1024 * 1024 ? `${s / 1024 / 1024}MB` : `${s / 1024}KB`}`)
      .join(", ");

    const result = {
      sizeBreakdown,
      totalMB: (sizes.reduce((a, b) => a + b, 0) / 1024 / 1024).toFixed(1),
      transferId: transfer.id,
      status: status?.status,
      fileCount: netappPaths.length,
      fileSizeBytes: manifests[0].fileSizeBytes,
      netappSourceFolders: manifests.map((m) => m.netappFolder),
      egressDestination,
      durationSeconds: Math.round(durationSeconds),
      durationMinutes: (durationSeconds / 60).toFixed(1),
      secondsPerFile: (durationSeconds / netappPaths.length).toFixed(2),
      filesPerMinute: (netappPaths.length / (durationSeconds / 60)).toFixed(1),
      successfulFiles: status?.successfulFiles,
      failedFiles: status?.failedFiles,
      seedDurationSeconds: manifests.reduce((s, m) => s + m.seedDurationSeconds, 0),
      progress: samples,
    };

    test.info().annotations.push({
      type: "Transfer Duration",
      description: `${result.durationMinutes} min (${result.secondsPerFile}s/file)`,
    });
    await test.info().attach("transfer-performance", {
      body: JSON.stringify(result, null, 2),
      contentType: "application/json",
    });

    console.log("\n=== SMALL-FILE NETAPP -> EGRESS ===");
    console.log(`Transfer ID: ${transfer.id}`);
    console.log(`Files: ${result.fileCount} (${result.sizeBreakdown}), ${result.totalMB} MB total`);
    console.log(`Duration: ${result.durationMinutes} min`);
    console.log(`Per file: ${result.secondsPerFile}s (${result.filesPerMinute} files/min)`);
    console.log(`Egress destination: ${egressDestination}`);
    console.log("===================================\n");

    await harness.clearTransfer(transfer.id);

    if (verifySizes) {
      await test.step("Verify every copied file's size in Egress", async () => {
        const expected = new Map(
          manifests.flatMap((m) =>
            m.fileNames.map((n) => [n, m.fileSizes?.[n] ?? m.fileSizeBytes] as const),
          ),
        );

        // Egress can lag listing freshly completed uploads; give it a few minutes.
        let copied = await harness.listEgressFolder(egressDestination);
        for (let i = 0; i < 5 && copied.length < expected.size; i++) {
          await new Promise((r) => setTimeout(r, 60_000));
          copied = await harness.listEgressFolder(egressDestination);
        }
        const actual = new Map(copied.map((f) => [f.fileName, f.fileSize]));

        const missing = [...expected.keys()].filter((n) => !actual.has(n));
        const wrongSize = [...expected]
          .filter(([n, size]) => actual.has(n) && actual.get(n) !== size)
          .map(([n, size]) => `${n} (expected ${size}, got ${actual.get(n)})`);

        console.log(
          `Size check: ${expected.size - missing.length - wrongSize.length}/${expected.size} ok, ` +
            `${missing.length} missing, ${wrongSize.length} wrong size`,
        );
        expect.soft(missing, "files missing from the Egress destination").toEqual([]);
        expect.soft(wrongSize, "files with a different size in Egress").toEqual([]);
      });
    }

    expect.soft(
      status,
      `Transfer ${transfer.id} finished as ${status?.status}, ` +
        `${status?.failedFiles}/${status?.totalFiles} file(s) failed` +
        (status?.errorMessage ? ` (${status.errorMessage})` : ""),
    ).toMatchObject({ status: "Completed", failedFiles: 0 });

    expect(
      durationSeconds / 60,
      `NetApp -> Egress copy of ${netappPaths.length} files took ${result.durationMinutes} min, ` +
        `target is ${targetMinutes} min`,
    ).toBeLessThanOrEqual(targetMinutes);
  });
});
