import { useCallback, useState } from "react";

type TransferMaterialLocalState = {
  transferDirection: "egressToSharedDrive" | "sharedDriveToEgress";
};

const TransferMaterialsNew = () => {
  const initialState: TransferMaterialLocalState = {
    transferDirection: "egressToSharedDrive",
  };
  const [localState, setLocalState] =
    useState<TransferMaterialLocalState>(initialState);

  const getMainTexts = useCallback(() => {
    if (localState.transferDirection === "egressToSharedDrive") {
      return {
        title: "Transfer from Egress to the Shared Drive",
        description:
          "Select the files or folders you want to transfer. Then choose where to save them on the Shared Drive.",
      };
    } else {
      return {
        title: "Transfer from Shared Drive to Egress",
        description:
          "Select the files or folders you want to transfer. Then choose where to save them on Egress.",
      };
    }
  }, [localState.transferDirection]);

  return (
    <div>
      <h2>{getMainTexts().title}</h2>
      <p>{getMainTexts().description}</p>
    </div>
  );
};

export default TransferMaterialsNew;
