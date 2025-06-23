import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusPlaywright: TransferStatusResponse = {
  overallStatus: "COMPLETED",
  transferType: "COPY",
  direction: "EgressToNetApp",
  completedAt: null,
  failedFiles: [],
  username: "dev_user@example.org",
};

export const netAppToEgressTransferStatusPlaywright: TransferStatusResponse = {
  overallStatus: "COMPLETED",
  transferType: "COPY",
  direction: "NetAppToEgress",
  completedAt: null,
  failedFiles: [],
  username: "dev_user@example.org",
};
