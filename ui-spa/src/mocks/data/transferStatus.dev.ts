import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  overallStatus: "COMPLETED",
  transferType: "COPY",
  direction: "EgressToNetApp",
  completedAt: null,
  failedFiles: [],
  username: "dev_user@example.org",
};

export const netAppToEgressTransferStatusDev: TransferStatusResponse = {
  overallStatus: "COMPLETED",
  transferType: "COPY",
  direction: "NetAppToEgress",
  completedAt: null,
  failedFiles: [],
  username: "dev_user@example.org",
};
