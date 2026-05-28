import { useState, useCallback } from "react";
import { Button, BackLink, LinkButton, NotificationBanner } from "../../govuk";
import TreeViewComponent, {
  type TreeNode,
} from "../../common/tree-view-component/TreeViewComponent";
import { getNetAppFolders } from "../../../apis/gateway-api";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import styles from "./ManageMaterialsDestinationPicker.module.scss";

type ManageMaterialsDestinationPickerProps = {
  netAppPath: string;
  operationName: string;
  action: "copy" | "move";
  selectedCount: number;
  conflictError: string | null;
  onConfirm: (destinationPath: string, destinationName: string) => void;
  onCancel: () => void;
};

const ManageMaterialsDestinationPicker: React.FC<
  ManageMaterialsDestinationPickerProps
> = ({
  netAppPath,
  operationName,
  action,
  selectedCount,
  conflictError,
  onConfirm,
  onCancel,
}) => {
  const [selectedDestination, setSelectedDestination] =
    useState<TreeNode | null>(null);

  const actionLabel = action === "copy" ? "copying" : "moving";
  const actionVerb = action === "copy" ? "copy" : "move";
  const actionTitle = action === "copy" ? "Copy" : "Move";

  const rootNode: TreeNode[] = [
    {
      id: netAppPath,
      name: `Shared Drive: ${operationName}`,
      isFolder: true,
    },
  ];

  const handleLoadChildren = useCallback(
    async (nodeId: string): Promise<TreeNode[]> => {
      const data = await getNetAppFolders(nodeId);
      return data.folderData.map((folder) => ({
        id: folder.path,
        name: getFolderNameFromPath(folder.path),
        isFolder: true,
      }));
    },
    [],
  );

  const handleSelect = useCallback((node: TreeNode) => {
    if (node.isFolder) {
      setSelectedDestination(node);
    }
  }, []);

  const handleConfirm = () => {
    if (selectedDestination) {
      onConfirm(selectedDestination.id, selectedDestination.name);
    }
  };

  const itemText = selectedCount === 1 ? "item" : "items";

  return (
    <div>
      <BackLink onClick={onCancel} href="#">
        Back
      </BackLink>

      <h2 className="govuk-heading-l">Choose a Shared Drive folder</h2>
      <p className="govuk-body">
        You are {actionLabel} {selectedCount} {itemText}.
      </p>
      <p className="govuk-body">
        Select the Shared Drive folder you want to {actionVerb} them into.
      </p>

      {conflictError && (
        <NotificationBanner type="important">
          {conflictError.split("\n").map((msg, i) => (
            <p key={i} className="govuk-body">
              {msg}
            </p>
          ))}
        </NotificationBanner>
      )}

      <div className={styles.treeWrapper}>
        <TreeViewComponent
          data={rootNode}
          onSelect={handleSelect}
          onLoadChildren={handleLoadChildren}
        />
      </div>

      <div className={styles.actionWrapper}>
        <Button
          disabled={!selectedDestination}
          onClick={handleConfirm}
        >
          {selectedDestination
            ? `${actionTitle} to ${selectedDestination.name}`
            : actionTitle}
        </Button>
        <LinkButton onClick={onCancel}>Cancel</LinkButton>
      </div>
    </div>
  );
};

export default ManageMaterialsDestinationPicker;
