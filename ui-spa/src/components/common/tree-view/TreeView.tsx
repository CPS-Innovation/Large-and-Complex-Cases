import React, {
  useState,
  useCallback,
  useRef,
  useEffect,
  KeyboardEvent,
} from "react";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import { Spinner } from "../Spinner";
import styles from "./TreeView.module.scss";

export type TreeNode = {
  id: string;
  name: string;
  path?: string;
  isFolder: boolean;
  children?: TreeNode[];
};

export type TreeViewProps = {
  data: TreeNode[];
  onSelect?: (node: TreeNode) => void;
  onLoadChildren?: (nodeId: string) => Promise<TreeNode[]>;
  selectedId?: string | null;
  className?: string;
};

const TreeView: React.FC<TreeViewProps> = ({
  data,
  onSelect,
  onLoadChildren,
  selectedId: controlledSelectedId,
  className,
}) => {
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set([]));
  const [focusedId, setFocusedId] = useState<string | null>(() => {
    return data.length > 0 ? data[0].id : null;
  });
  const [uncontrolledSelectedId, setUncontrolledSelectedId] = useState<
    string | null
  >(null);

  const [loadingIds, setLoadingIds] = useState<Set<string>>(new Set());
  const [loadedChildren, setLoadedChildren] = useState<
    Record<string, TreeNode[]>
  >({});

  const selectedId = controlledSelectedId ?? uncontrolledSelectedId;

  const refs = useRef<Record<string, HTMLElement | null>>({});

  const getVisibleNodes = useCallback(() => {
    const out: TreeNode[] = [];
    const walk = (nodes: TreeNode[]) => {
      for (const n of nodes) {
        out.push(n);
        const children = n.children ?? loadedChildren[n.id];
        if (n.isFolder && expandedIds.has(n.id) && children) {
          walk(children);
        }
      }
    };
    walk(data);
    return out;
  }, [data, expandedIds, loadedChildren]);

  const findParentId = useCallback(
    (nodes: TreeNode[], childId: string): string | null => {
      for (const n of nodes) {
        const children = n.children ?? loadedChildren[n.id];
        if (children?.some((c) => c.id === childId)) return n.id;
        if (children) {
          const p = findParentId(children, childId);
          if (p) return p;
        }
      }
      return null;
    },
    [loadedChildren],
  );

  const findNodeById = useCallback(
    (nodes: TreeNode[], id: string): TreeNode | undefined => {
      for (const n of nodes) {
        if (n.id === id) return n;
        if (n.children) {
          const found = findNodeById(n.children, id);
          if (found) return found;
        }
        const cached = loadedChildren[n.id];
        if (cached) {
          const found = findNodeById(cached, id);
          if (found) return found;
        }
      }
      return undefined;
    },
    [loadedChildren],
  );

  const toggle = useCallback(
    (id: string) => {
      console.log("Toggling node:", id);
      setExpandedIds((prev) => {
        const next = new Set(prev);
        if (next.has(id)) {
          next.delete(id);
        } else {
          next.add(id);
          const node = findNodeById(data, id);

          if (node) {
            const hasChildren =
              (node.children && node.children.length > 0) ||
              (loadedChildren?.[id] && loadedChildren[id].length > 0);

            if (!hasChildren && onLoadChildren) {
              setLoadingIds((s) => new Set(s).add(id));

              onLoadChildren(node.id)
                .then((children: TreeNode[]) => {
                  setLoadedChildren((prev) => ({ ...prev, [id]: children }));
                })
                .catch(() => {
                  console.error("Error loading children for node:", id);
                })
                .finally(() => {
                  setLoadingIds((s) => {
                    const nextSet = new Set(s);
                    nextSet.delete(id);
                    return nextSet;
                  });
                });
            }
          }
        }
        return next;
      });
    },
    [data, findNodeById, onLoadChildren, loadedChildren],
  );

  useEffect(() => {
    if (focusedId) {
      const el = refs.current[focusedId];
      el?.focus();
    }
  }, [focusedId]);

  const visible = getVisibleNodes();

  const focusNext = (id: string | null) => {
    if (!id) return;
    const i = visible.findIndex((n) => n.id === id);
    if (i >= 0 && i < visible.length - 1) setFocusedId(visible[i + 1].id);
  };

  const focusPrev = (id: string | null) => {
    if (!id) return;
    const i = visible.findIndex((n) => n.id === id);
    if (i > 0) setFocusedId(visible[i - 1].id);
  };

  const focusFirst = () => {
    if (visible.length > 0) setFocusedId(visible[0].id);
  };
  const focusLast = () => {
    const last = visible.at(-1);
    if (last) setFocusedId(last.id);
  };

  const onKeyDown = (e: KeyboardEvent) => {
    if (!focusedId) return;
    const node = findNodeById(data, focusedId);
    if (!node) return;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        focusNext(focusedId);
        break;
      case "ArrowUp":
        e.preventDefault();
        focusPrev(focusedId);
        break;
      case "ArrowRight":
        e.preventDefault();
        if (node.isFolder) {
          if (!expandedIds.has(node.id)) {
            toggle(node.id);
          } else if (node.children && node.children.length > 0) {
            setFocusedId(node.children[0].id);
          } else if (
            loadedChildren[node.id] &&
            loadedChildren[node.id].length > 0
          ) {
            setFocusedId(loadedChildren[node.id][0].id);
          }
        }
        break;
      case "ArrowLeft":
        e.preventDefault();
        if (node.isFolder && expandedIds.has(node.id)) {
          toggle(node.id);
        } else {
          const parentId = findParentId(data, node.id);
          if (parentId) setFocusedId(parentId);
        }
        break;
      case "Home":
        e.preventDefault();
        focusFirst();
        break;
      case "End":
        e.preventDefault();
        focusLast();
        break;
      case "Enter":
      case " ":
        e.preventDefault();
        if (!controlledSelectedId) setUncontrolledSelectedId(node.id);
        onSelect?.(node);

        break;
      default:
        break;
    }
  };

  const handleClick = (node: TreeNode) => {
    setFocusedId(node.id);
    if (!controlledSelectedId) setUncontrolledSelectedId(node.id);
    onSelect?.(node);
  };

  const renderNode = (node: TreeNode, level: number) => {
    const isExpanded = node.isFolder && expandedIds.has(node.id);
    const isSelected = selectedId === node.id;
    const isFocused = focusedId === node.id;
    const isLoading = loadingIds.has(node.id);
    const children = node.children ?? loadedChildren[node.id];

    return (
      <li
        key={node.id + level}
        role="treeitem"
        ref={(el) => {
          refs.current[node.id] = el;
        }}
        aria-expanded={node.isFolder ? isExpanded : undefined}
        aria-busy={isLoading ? true : undefined}
        aria-level={level}
        data-id={node.id}
        aria-selected={isSelected}
        aria-labelledby={`name-${node.id}`}
        tabIndex={isFocused ? 0 : -1}
        className={`${styles.treeItem} ${isFocused ? styles.focused : ""}`}
        onKeyDown={(e) => {
          e.stopPropagation();
          onKeyDown(e);
        }}
      >
        <div className={styles.node}>
          <div aria-live="polite" className="govuk-visually-hidden">
            {isLoading ? "Loading sub folders" : ""}
          </div>

          <button
            onClick={() => toggle(node.id)}
            className={styles.toggleIcon}
            aria-label={isExpanded ? "minus" : "plus"}
            tabIndex={-1}
          >
            {isExpanded ? "-" : "+"}
          </button>
          <button
            id={`name-${node.id}`}
            className={`folderNode ${styles.folderNode} ${isSelected ? styles.selected : ""}`}
            onClick={() => handleClick(node)}
            aria-label={node.name.toLowerCase()}
            tabIndex={-1}
          >
            <div aria-hidden={true}>
              <FolderIcon />
            </div>
            {node.name}
          </button>
        </div>
        {isLoading && (
          <div className={styles.loadingIconWrapper} aria-hidden>
            <Spinner data-testid="transfer-spinner" diameterPx={15} />
          </div>
        )}
        {!isLoading &&
          node.isFolder &&
          isExpanded &&
          children &&
          children.length > 0 && (
            <ul role="group" className={styles.children}>
              {children.map((child) => renderNode(child, level + 1))}
            </ul>
          )}
      </li>
    );
  };

  return (
    <div className={`${styles.tree} ${className ?? ""}`}>
      <ul role="tree" aria-label="Folders" className={styles.treeList}>
        {data.map((node) => renderNode(node, 1))}
      </ul>
    </div>
  );
};

export default TreeView;
