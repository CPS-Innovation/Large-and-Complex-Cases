import { useCallback, useState } from "react";
import TreeView from "../tree-view-component/TreeViewComponent";
import { Button, LinkButton } from "../../../components/govuk";
import { type TreeNode } from "../tree-view-component/TreeViewComponent";
import styles from "./TransferWidget.module.scss";
export type TransferWidgetProps = {
  data: TreeNode[];
  onLoadChildren: (nodeId: string) => Promise<TreeNode[]>;
  transferAction: "Copy" | "Move";
  handleCancelClick: () => void;
  handleTransfer: (selectedNode: TreeNode) => void;
  isRootNodeOpened: boolean;
};
const TransferWidget: React.FC<TransferWidgetProps> = ({
  data,
  transferAction,
  isRootNodeOpened,
  handleCancelClick,
  onLoadChildren,
  handleTransfer,
}) => {
  const [selectedNode, setSelectedNode] = useState<TreeNode | null>(null);
  const onSelect = useCallback((node: TreeNode) => {
    setSelectedNode(node);
  }, []);

  const handleBtnClickHandler = () => {
    if (selectedNode) {
      handleTransfer(selectedNode);
    }
  };

  return (
    <div>
      <h3>Transfer Files</h3>
      <TreeView
        data={data}
        onSelect={onSelect}
        onLoadChildren={onLoadChildren}
        isRootNodeOpened={isRootNodeOpened}
      />
      <div className={styles.actionWrapper}>
        <Button
          onClick={handleBtnClickHandler}
          className={styles.transferButton}
          disabled={!selectedNode}
        >
          {selectedNode
            ? `${transferAction} to ${selectedNode?.name}`
            : transferAction}
        </Button>

        <LinkButton onClick={handleCancelClick}>Cancel</LinkButton>
      </div>
    </div>
  );
};

export default TransferWidget;
