export type TransferAction = {
  destinationFolder: {
    path: string;
    name: string;
    sourceType: "egress" | "netapp";
  };
  actionType: "move" | "copy";
};
