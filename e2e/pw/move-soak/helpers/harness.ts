import type {
  HarnessConfig,
  FileBatchSpec,
  TransferSourcePath,
  InitiateTransferPayload,
  InitiateTransferResponse,
  TransferStatusCheckResponse,
} from "./types";
import type { UploadedFile } from "../../helpers/types";
import {
  authenticateEgress,
  uploadFile,
  getUploadedFile,
} from "../../helpers/egress-api";
import { getAzureADToken } from "../../helpers/auth-api";

// Renew the shared Egress token well inside its ~10 minute lifetime.
const EGRESS_TOKEN_REFRESH_MS = 7 * 60 * 1000;

// Stop waiting on Egress to finalise staged uploads once the count has not
// grown for this long.
const STAGING_STALL_MS = 10 * 60 * 1000;

export class MoveSoakHarness {
  constructor(private readonly config: HarnessConfig) {}

  private readonly correlationId = crypto.randomUUID();
  private egressToken!: string;

  private aadAccessToken?: string;

  async setupEgressAuth() {
    this.egressToken = await authenticateEgress(
      this.config.egressBaseUrl,
      this.config.serviceAccountAuth
    );
    this.egressTokenIssuedAt = Date.now();

    return this.egressToken;
  }

  // Egress tokens last ~10 minutes and repeated auth calls get the caller's
  // IP temporarily blocked. Concurrent uploads therefore share one token that
  // is renewed once, ahead of expiry, rather than each upload discovering the
  // expiry via a 401 and re-authenticating on its own.
  private egressTokenIssuedAt = 0;
  private egressTokenRefresh?: Promise<string>;

  private async getEgressToken(): Promise<string> {
    if (Date.now() - this.egressTokenIssuedAt < EGRESS_TOKEN_REFRESH_MS) {
      return this.egressToken;
    }

    this.egressTokenRefresh ??= this.setupEgressAuth().finally(() => {
      this.egressTokenRefresh = undefined;
    });

    return this.egressTokenRefresh;
  }

  async setupAadAuth() {
    this.aadAccessToken = await getAzureADToken(
      this.config.tenantId,
      this.config.apiClientId,
      this.config.aadUsername,
      this.config.aadPassword
    );

    return this.aadAccessToken;
  }

  private ensureEgressAuth() {
    if (!this.egressToken) {
      throw new Error("Egress auth not set up. Call setupEgressAuth() first.");
    }
  }

  private ensureAadAuth() {
    if (!this.aadAccessToken) {
      throw new Error("LCC auth not set up. Call setupAadAuth() first.");
    }
  }

  private getLccApiHeaders() {
    this.ensureAadAuth();

    return {
      Authorization: `Bearer ${this.aadAccessToken}`,
      // "Content-Type": "application/json",
      "Correlation-Id" : this.correlationId,
    };
  }

  // Calls the LCC API, re-acquiring the AAD token once on 401 so polls of
  // multi-hour transfers survive the ~1h token lifetime.
  private async lccFetch(url: string, init: RequestInit = {}): Promise<Response> {
    const send = () =>
      fetch(url, {
        ...init,
        headers: { ...this.getLccApiHeaders(), ...(init.headers ?? {}) },
      });

    const res = await send();
    if (res.status !== 401) return res;

    console.log("LCC API returned 401 - refreshing AAD token and retrying");
    await this.setupAadAuth();
    return send();
  }

  // Upload files to Egress. `concurrency` bounds how many uploads (and
  // upload-status polls) are in flight at once; the default of 1 keeps the
  // original sequential behaviour for the multi-GB soak scenarios.
  async stageFiles(
    fileSpecs: FileBatchSpec[],
    { concurrency = 1 }: { concurrency?: number } = {},
  ): Promise<UploadedFile[]> {
    this.ensureEgressAuth();

    type Upload = {
      uploadId: string;
      sizeMb: number;
    };

    const runId = Date.now();
    const pending = fileSpecs.flatMap((spec) =>
      Array.from({ length: spec.fileCount }, () => spec),
    );

    const uploadIds: Upload[] = await mapWithConcurrency(
      pending,
      concurrency,
      async (spec, fileIndex) => {
        const sizeLabel =
          spec.fileSizeMb >= 1
            ? `${spec.fileSizeMb}MB`
            : `${Math.round(spec.fileSizeMb * 1024)}KB`;

        const uploadId = await uploadFile(
          this.config.egressBaseUrl,
          await this.getEgressToken(),
          this.config.serviceAccountAuth,
          this.config.workspaceId,
          Math.round(spec.fileSizeMb * 1024 * 1024),
          `soak-${sizeLabel}-${runId}-${fileIndex}.txt`,
          { folderPath: this.config.egressSourceFolder },
        );

        return { uploadId, sizeMb: spec.fileSizeMb };
      },
    );

    // get new Egress Token to avoid expiry during large batch poll:
    await this.setupEgressAuth();

    if (concurrency === 1) {
      return Promise.all(
        uploadIds.map(async (Upload) =>
          getUploadedFile(
            this.config.egressBaseUrl,
            await this.getEgressToken(),
            this.config.serviceAccountAuth,
            this.config.workspaceId,
            Upload.uploadId,
            {
              timeoutMs: Math.max(30000, Upload.sizeMb * 15000),
              retryDelay: Math.min(10000,Math.max(1000, Upload.sizeMb * 5)),
            }
          )
        )
      );
    }

    // High-volume staging: Egress turns thousands of uploads into files slowly
    // (~35/min observed), far outlasting a per-upload status poll. Wait on the
    // folder listing instead, and give up on stragglers once it stops growing.
    const runMarker = `-${runId}-`;
    const isThisRun = (name: string) => name.includes(runMarker);
    let files: UploadedFile[] = [];
    let lastGrowth = Date.now();

    while (true) {
      const listed = await this.listEgressFolder(this.config.egressSourceFolder, isThisRun);
      if (listed.length > files.length) lastGrowth = Date.now();
      files = listed;
      console.log(`Egress has finalised ${files.length}/${uploadIds.length} upload(s)`);

      if (files.length >= uploadIds.length) break;
      if (Date.now() - lastGrowth > STAGING_STALL_MS) {
        console.warn(
          `Egress stopped finalising uploads - continuing with ${files.length}/${uploadIds.length}`,
        );
        break;
      }
      await new Promise((r) => setTimeout(r, 60_000));
    }

    return files;
  }

  // Lists files already in the Egress source folder whose names start with
  // `namePrefix`, so a staged batch can be reused without re-uploading.
  async listStagedFiles(namePrefix: string): Promise<UploadedFile[]> {
    return this.listEgressFolder(this.config.egressSourceFolder, (name) =>
      name.startsWith(namePrefix),
    );
  }

  // Lists the files in an Egress folder (path with or without a trailing
  // slash) whose names match `include`.
  async listEgressFolder(
    folderPath: string,
    include: (fileName: string) => boolean = () => true,
  ): Promise<UploadedFile[]> {
    // Egress matches the path only without its trailing slash.
    const folder = folderPath.replace(/\/$/, "");
    const files: UploadedFile[] = [];

    for (let skip = 0; ; skip += 100) {
      const res = await fetch(
        `${this.config.egressBaseUrl}/api/v1/workspaces/${this.config.workspaceId}/files` +
          `?view=full&skip=${skip}&limit=100&path=${encodeURIComponent(folder)}`,
        { headers: { Authorization: `Basic ${await this.getEgressToken()}` } },
      );
      if (!res.ok) {
        throw new Error(`Failed to list '${folder}': ${res.status} ${await res.text()}`);
      }

      const items: {
        id: string;
        filename: string;
        filesize?: number;
        file_size?: number;
        is_folder?: boolean;
        parent_folder_id?: string;
      }[] = (await res.json()).data ?? [];

      for (const item of items) {
        if (!item.is_folder && include(item.filename)) {
          files.push({
            fileId: item.id,
            fileName: item.filename,
            fileSize: item.filesize ?? item.file_size ?? 0,
            parentFolderId: item.parent_folder_id ?? "",
          });
        }
      }

      if (items.length < 100) break;
    }

    return files;
  }

  async validateTransfer(files: UploadedFile[]): Promise<void> {
    const sourcePaths: TransferSourcePath[] = files.map((file) => ({
      fileId: file.fileId,
      path: `${this.config.egressSourceFolder}${file.fileName}`,
      isFolder: false
    }));

    const payload: InitiateTransferPayload = {
      caseId: this.config.caseId,
      transferDirection: "EgressToNetApp",
      transferType: "Move",
      sourcePaths,
      destinationPath: this.config.netappFolderPath,
      workspaceId: this.config.workspaceId,
      sourceRootFolderPath: this.config.egressSourceFolder,
    };

    const res = await fetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/files`,
      {
        method: "POST",
        headers: this.getLccApiHeaders(),
        body: JSON.stringify(payload),
      }
    );

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to initiate move: ${res.status} ${res.statusText} - ${body}`
      );
    }

    const validation = await res.json();
    console.log(`Transfer validation isInvalid: ${validation.isInvalid}`);
  }

  // Move files to NetApp using Transfer API
  async startMove(
    files: UploadedFile[],
    destinationPath = this.config.netappFolderPath,
  ): Promise<InitiateTransferResponse> {
    const sourcePaths: TransferSourcePath[] = files.map((file) => ({
      fileId: file.fileId,
      path: file.fileName,
      fullFilePath: `${this.config.egressSourceFolder}${file.fileName}`,
    }));

    return this.initiateTransfer({
      caseId: this.config.caseId,
      transferDirection: "EgressToNetApp",
      transferType: "Move",
      sourcePaths,
      destinationPath,
      workspaceId: this.config.workspaceId,
      sourceRootFolderPath: this.config.egressSourceFolder,
    });
  }

  // Copy NetApp files (full NetApp keys) into an Egress folder. Same payload
  // shape as the Postman "11. Initiate Transfer (NetApp to Egress)" step.
  async startNetAppToEgressCopy(
    netappFilePaths: string[],
    egressDestinationFolder: string,
  ): Promise<InitiateTransferResponse> {
    const sourcePaths: TransferSourcePath[] = netappFilePaths.map((path) => ({
      path,
      relativePath: path.split("/").pop(),
    }));

    return this.initiateTransfer({
      caseId: this.config.caseId,
      transferDirection: "NetAppToEgress",
      transferType: "Copy",
      sourcePaths,
      destinationPath: egressDestinationFolder,
      workspaceId: this.config.workspaceId,
      sourceRootFolderPath: "",
    });
  }

  private async initiateTransfer(
    payload: InitiateTransferPayload,
  ): Promise<InitiateTransferResponse> {
    const res = await this.lccFetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/initiate`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Accept": "*/*"
        },
        body: JSON.stringify(payload),
      }
    );

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to initiate ${payload.transferDirection} ${payload.transferType}: ${res.status} ${res.statusText} - ${body}`
      );
    }

    const responseBody = JSON.parse(await res.text()) as InitiateTransferResponse;

    console.log(
      `Transfer initiated: ${responseBody.id} (${responseBody.status})`
    );

    return responseBody;
  }

  // Check transfer status
  async checkTransferStatus(
    transferId: InitiateTransferResponse["id"]
  ): Promise<TransferStatusCheckResponse | null> {
    const res = await this.lccFetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/${transferId}/status`,
      { method: "GET" }
    );

    if (res.status === 404) {
      return null;
    }

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to check transfer status: ${res.status} ${res.statusText} - ${body}`
      );
    }

    const resBody = await res.json();

    return {
      id: resBody.id,
      status: resBody.status,
      failedItems: resBody.failedItems.map(
        (item: { sourcePath: string }) => item.sourcePath
      ),
      successfulItems: resBody.successfulItems.map(
        (item: { sourcePath: string }) => item.sourcePath
      ),
      totalFiles: resBody.totalFiles,
      processedFiles: resBody.processedFiles,
      successfulFiles: resBody.successfulFiles,
      failedFiles: resBody.failedFiles,
      errorMessage: resBody.errorMessage ?? null,
    } as TransferStatusCheckResponse;
  }

  // Clear transfer
  async clearTransfer(
    transferId: InitiateTransferResponse["id"]
  ): Promise<void> {
    const res = await this.lccFetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/${transferId}/clear`,
      { method: "POST" }
    );

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to clear transfer: ${res.status} ${res.statusText} - ${body}`
      );
    }
  }
}

// Runs `fn` over `items` with at most `limit` calls in flight, preserving
// input order in the result.
async function mapWithConcurrency<T, R>(
  items: T[],
  limit: number,
  fn: (item: T, index: number) => Promise<R>,
): Promise<R[]> {
  const results: R[] = new Array(items.length);
  let next = 0;

  const worker = async () => {
    while (next < items.length) {
      const index = next++;
      results[index] = await fn(items[index], index);
    }
  };

  await Promise.all(
    Array.from({ length: Math.max(1, Math.min(limit, items.length)) }, worker),
  );

  return results;
}
