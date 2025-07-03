import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusPlaywright: TransferStatusResponse = {
  status: "Completed",
  transferType: "COPY",
  direction: "EgressToNetApp",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
};

export const netAppToEgressTransferStatusPlaywright: TransferStatusResponse = {
  status: "Completed",
  transferType: "COPY",
  direction: "NetAppToEgress",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
};
