import { useCallback, useState } from "react";
import TreeView from "../tree-view-component/TreeView";
import { Button } from "../../../components/govuk/Button";
import { Link } from "react-router-dom";
import { type TreeNode } from "../tree-view-component/TreeView";
import styles from "./TransferWidget.module.scss";
export type TransferWidgetProps = {
  data: TreeNode[];
  onLoadChildren: (nodeId: string) => Promise<TreeNode[]>;
  transferAction: "Copy" | "Move";
};
const TransferWidget: React.FC<TransferWidgetProps> = ({
  data,
  onLoadChildren,
  transferAction,
}) => {
  const [selectedNode, setSelectedNode] = useState<TreeNode | null>(null);
  const onSelect = useCallback((node: TreeNode) => {
    setSelectedNode(node);
  }, []);

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
          onClick={() => console.log("Transfer files")}
          className={styles.transferButton}
        >
          {selectedNode
            ? `${transferAction} to ${selectedNode?.name}`
            : transferAction}
        </Button>
        <Link to="/somewhere-else" className="govuk-link--no-visited-state">
          Cancel
        </Link>
      </div>
    </div>
  );
};

export default TransferWidget;
