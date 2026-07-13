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
  listEgressWorkspaceFilesByFolderId,
  deleteFiles,
} from "../../helpers/egress-api";
import { getAzureADToken } from "../../helpers/auth-api";

export class MoveSoakHarness {
  constructor(private config: HarnessConfig) {}

  private correlationId = crypto.randomUUID();
  private egressToken!: string;

  private aadAccessToken?: string;

  // Failure injection state for "AAD token expires during status polling"
  private failStatusPollingAfterChecks?: number;
  private statusCheckCount = 0;

  async setupEgressAuth() {
    this.egressToken = await authenticateEgress(
      this.config.egressBaseUrl,
      this.config.serviceAccountAuth
    );
    
    return this.egressToken;
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

  enableTokenExpiryDuringStatusPolling(afterChecks: number) {
    if (afterChecks < 1) {
      throw new Error("afterChecks must be >= 1");
    }

    this.failStatusPollingAfterChecks = afterChecks;
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

  private invalidateAadTokenForStatusPolling() {
    if (
      this.failStatusPollingAfterChecks !== undefined &&
      this.statusCheckCount >= this.failStatusPollingAfterChecks
    ) {
      // Deliberately replace the cached token so subsequent status requests
      // fail, simulating an expired/invalid AAD token after a
      // successful initiation.
      this.aadAccessToken = "expired-or-invalid-token";
    }
  }

  // Upload files to Egress
  async stageFiles(fileSpecs: FileBatchSpec[]): Promise<UploadedFile[]> {
    this.ensureEgressAuth();

    type Upload = {
      uploadId: string;
      sizeMb: number;
    };

    const uploadIds: Upload[] = [];

    let fileIndex = 0;

    for (const spec of fileSpecs) {
      for (let i = 0; i < spec.fileCount; i++) {
        const uploadId = await uploadFile(
          this.config.egressBaseUrl,
          this.egressToken,
          this.config.serviceAccountAuth,
          this.config.workspaceId,
          spec.fileSizeMb * 1024 * 1024,
          `soak-${spec.fileSizeMb}MB-${Date.now()}-${fileIndex}.txt`,
          this.config.egressSourceFolder,
        );

        uploadIds.push({ 
          uploadId: uploadId,
          sizeMb: spec.fileSizeMb
        });
        fileIndex++;
      }
    }

    // get new Egress Token to avoid expiry during large batch poll:
    const freshToken = await this.setupEgressAuth();

    const uploadedFiles = await Promise.all(
      uploadIds.map(Upload =>
        getUploadedFile(
          this.config.egressBaseUrl,
          freshToken,
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

    return uploadedFiles;
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

    console.log(JSON.stringify(payload))

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

    console.log(await res.json())
  }

  // Move files to NetApp using Transfer API
  async startMove(files: UploadedFile[]): Promise<InitiateTransferResponse> {
    const sourcePaths: TransferSourcePath[] = files.map((file) => ({
      fileId: file.fileId,
      path: file.fileName,
      fullFilePath: `${this.config.egressSourceFolder}${file.fileName}`,
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

    console.log(JSON.stringify(payload))

    const res = await fetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/initiate`,
      {
        method: "POST",
        headers: {
          ...this.getLccApiHeaders(),
          "Content-Type": "application/json",
          "Accept": "*/*"
        },
        body: JSON.stringify(payload),
      }
    );

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to initiate move: ${res.status} ${res.statusText} - ${body}`
      );
    }

    const responseText = await res.text();

    console.log("Initiate response:");
    console.log(responseText);

    const responseBody = JSON.parse(responseText);

    return responseBody as InitiateTransferResponse;

    // return (await res.json()) as InitiateTransferResponse;
  }

  // Check transfer status
  async checkTransferStatus(
    transferId: InitiateTransferResponse["id"]
  ): Promise<TransferStatusCheckResponse | null> {
    this.statusCheckCount++;
    this.invalidateAadTokenForStatusPolling();

    const res = await fetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/${transferId}/status`,
      {
        method: "GET",
        headers: this.getLccApiHeaders(),
      }
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
    } as TransferStatusCheckResponse;
  }

  // Clear transfer
  async clearTransfer(
    transferId: InitiateTransferResponse["id"]
  ): Promise<void> {
    const res = await fetch(
      `${this.config.apiBaseUrl}/api/v1/filetransfer/${transferId}/clear`,
      {
        method: "POST",
        headers: this.getLccApiHeaders(),
      }
    );

    if (!res.ok) {
      const body = await res.text();
      throw new Error(
        `Failed to clear transfer: ${res.status} ${res.statusText} - ${body}`
      );
    }
  }
}