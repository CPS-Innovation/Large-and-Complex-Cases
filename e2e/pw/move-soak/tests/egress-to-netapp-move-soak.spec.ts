import { test, expect } from "@playwright/test";
import { loadEnvConfig } from "../../helpers/env-config";
import { MoveSoakHarness } from "../helpers/harness";
import { moveSoakScenarios } from "./move-soak.scenarios";
import {
  verifyNetAppFileSizeByName,
  isFileInEgress,
} from "../../helpers/transfer-verify";

test.describe("Move Soak Tests", () => {
  test.setTimeout(90 * 60 * 1000);

  const config = loadEnvConfig();
  const egressSourceFolder = "1. ABEs for Transcript/";
  const caseId = Number(config.defaultCaseId!);

  for (const scenario of moveSoakScenarios) {
    test(scenario.name, async () => {
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

      const egressToken = await harness.setupEgressAuth();
      const aadAccessToken = await harness.setupAadAuth();

      // 1. Stage files
      const files = await harness.stageFiles(scenario.specs);

      // Validate Transfer
      await harness.validateTransfer(files);

      // Calculate transfer size before starting
      const totalBytes = files.reduce(
        (sum, file) => sum + file.fileSize,
        0
      );

      // 2. Start transfer
      const transferStart = Date.now();
      const transfer = await harness.startMove(files);

      // 3. Poll until completion
      let status;
      const timeoutMs = scenario.timeout;
      const start = Date.now();

      if (scenario.injectFailure) {
        harness.enableTokenExpiryDuringStatusPolling(10);
      }

      while (true) {
        console.log("Checking transfer status...");

        status = await harness.checkTransferStatus(transfer.id);

        console.log(
          "Transfer status response:\n",
          JSON.stringify(status, null, 2)
        );

        if (status?.status === "Completed") break;
        if (status?.status === "PartiallyCompleted") break;
        if (status?.status === "Failed") break;

        if (Date.now() - start > timeoutMs) {
          throw new Error("Transfer timed out");
        }

        await new Promise((r) => setTimeout(r, 5000));
      }

      const transferDurationMs = Date.now() - transferStart;
      const transferDurationSeconds = transferDurationMs / 1000;
      const transferDurationMinutes = transferDurationSeconds / 60;

      const throughputMBps =
        totalBytes /
        (1024 * 1024) /
        Math.max(transferDurationSeconds, 1);

      test.info().annotations.push({
        type: "Transfer Duration",
        description: `${transferDurationSeconds.toFixed(2)}s`,
      });

      test.info().annotations.push({
        type: "Transfer Throughput",
        description: `${throughputMBps.toFixed(2)} MB/s`,
      });

      await test.info().attach("transfer-performance", {
        body: JSON.stringify(
          {
            scenario: scenario.name,
            transferId: transfer.id,
            status: status?.status,
            fileCount: files.length,
            totalBytes,
            totalMB: (
              totalBytes /
              (1024 * 1024)
            ).toFixed(2),
            durationMs: transferDurationMs,
            durationSeconds: transferDurationSeconds.toFixed(2),
            durationMinutes: transferDurationMinutes.toFixed(2),
            throughputMBps: throughputMBps.toFixed(2),
            successfulFiles: status?.successfulFiles,
            failedFiles: status?.failedFiles,
          },
          null,
          2
        ),
        contentType: "application/json",
      });

      await test.info().attach("final-transfer-status", {
        body: JSON.stringify(status, null, 2),
        contentType: "application/json",
      });

      await test.step(
        `Transfer Performance: ${transferDurationSeconds.toFixed(
          2
        )}s (${throughputMBps.toFixed(2)} MB/s)`,
        async () => {}
      );

      console.log("\n=== TRANSFER PERFORMANCE ===");
      console.log(`Transfer ID: ${transfer.id}`);
      console.log(`Files: ${files.length}`);
      console.log(
        `Total Size: ${(totalBytes / (1024 * 1024)).toFixed(2)} MB`
      );
      console.log(
        `Duration: ${transferDurationSeconds.toFixed(2)}s`
      );
      console.log(
        `Throughput: ${throughputMBps.toFixed(2)} MB/s`
      );
      console.log("===========================\n");

      // 4. Cleanup transfer
      await harness.clearTransfer(transfer.id);

      // 5. Assertions per scenario
      expect(status).toBeTruthy();

      if (scenario.injectFailure) {
        expect(status?.status).toBe("Failed");

        // critical invariant: source NOT deleted
        expect(status?.failedFiles).toBeGreaterThan(0);
      } else {
        expect(status?.status).toBe("Completed");
        expect(status?.failedFiles).toBe(0);

        await test.step("Verify files exist in NetApp", async () => {
          for (const file of files) {
            await test.step(
              `Verify '${file.fileName}' exists in NetApp`,
              async () => {
                console.log(
                  `Verifying '${file.fileName}' exists in NetApp (${file.fileSize} bytes)`
                );

                await verifyNetAppFileSizeByName(
                  file.fileName,
                  caseId,
                  file.fileSize,
                );
              }
            );
          }
        });

        await test.step("Verify files removed from Egress", async () => {
          for (const file of files) {
            await test.step(
              `Verify '${file.fileName}' no longer exists in Egress`,
              async () => {
                console.log(
                  `Verifying '${file.fileName}' has been deleted from '${egressSourceFolder}'`
                );

                const exists = await isFileInEgress(
                  config.defaultWorkspaceId!,
                  file.parentFolderId,
                  file.fileName
                );

                expect(
                  exists,
                  `File '${file.fileName}' still exists in Egress`
                ).toBeFalsy();
              }
            );
          }
        });
      }
    });
  }
});