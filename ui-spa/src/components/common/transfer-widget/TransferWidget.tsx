import { useCallback, useState } from "react";
import TreeView from "../tree-view-component/TreeViewComponent";
import { Button } from "../../../components/govuk/Button";
import { Link } from "react-router-dom";
import { type TreeNode } from "../tree-view-component/TreeViewComponent";
import styles from "./TransferWidget.module.scss";
export type TransferWidgetProps = {
  data: TreeNode[];
  onLoadChildren: (nodeId: string) => Promise<TreeNode[]>;
  transferAction: "Copy" | "Move";
  cancelLink: string;
  handleTransfer: (selectedNode: TreeNode) => void;
};
const TransferWidget: React.FC<TransferWidgetProps> = ({
  data,
  transferAction,
  cancelLink,
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
        <Link to={cancelLink} className="govuk-link--no-visited-state">
          Cancel
        </Link>
      </div>
    </div>
  );
};

export default TransferWidget;
