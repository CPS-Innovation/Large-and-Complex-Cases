import React, {
  useState,
  useCallback,
  useRef,
  useEffect,
  KeyboardEvent,
} from "react";
import FolderIcon from "../../../components/svgs/folder.svg?react";
import FileIcon from "../../../components/svgs/file.svg?react";
import { Spinner } from "../Spinner";
import styles from "./TreeView.module.scss";

export type TreeNode = {
  id: string;
  name: string;
  isFolder: boolean;
  children?: TreeNode[];
};

export type TreeViewProps = {
  data: TreeNode[];
  onSelect?: (node: TreeNode) => void;
  onLoadChildren?: (nodeId: string) => Promise<TreeNode[]>;
  className?: string;
};

const TreeView: React.FC<TreeViewProps> = ({
  data,
  onSelect,
  onLoadChildren,
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

  const handleLoadingNodeChildren = useCallback(
    (node: TreeNode, id: string) => {
      const hasChildren =
        (node.children && node.children.length > 0) || loadedChildren?.[id];

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
    },
    [onLoadChildren, loadedChildren],
  );

  const toggle = useCallback(
    (id: string) => {
      setExpandedIds((prev) => {
        const next = new Set(prev);
        if (next.has(id)) {
          next.delete(id);
        } else {
          next.add(id);
          const node = findNodeById(data, id);

          if (node) {
            handleLoadingNodeChildren(node, id);
          }
        }
        return next;
      });
    },
    [data, findNodeById, handleLoadingNodeChildren],
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

  const openOrChangeFocus = (node: TreeNode) => {
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
  };

  const closeOrChangeFocus = (node: TreeNode) => {
    if (node.isFolder && expandedIds.has(node.id)) {
      toggle(node.id);
    } else {
      const parentId = findParentId(data, node.id);
      if (parentId) setFocusedId(parentId);
    }
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
        openOrChangeFocus(node);
        break;
      case "ArrowLeft":
        e.preventDefault();
        closeOrChangeFocus(node);
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
        if (node.isFolder) {
          e.preventDefault();
          setUncontrolledSelectedId(node.id);
          onSelect?.(node);
        }

        break;
      default:
        break;
    }
  };

  const handleClick = (node: TreeNode) => {
    setFocusedId(node.id);
    setUncontrolledSelectedId(node.id);
    onSelect?.(node);
  };

  const renderNode = (node: TreeNode, level: number) => {
    const isExpanded = node.isFolder && expandedIds.has(node.id);
    const isSelected = uncontrolledSelectedId === node.id;
    const isFocused = focusedId === node.id;
    const isLoading = loadingIds.has(node.id);
    const children = node.children ?? loadedChildren[node.id];

    const domId = crypto.randomUUID();
    const dataTestId = node.id.toLowerCase().replace(/\s+/g, "-");

    return (
      <li
        key={node.id}
        role="treeitem"
        ref={(el) => {
          refs.current[node.id] = el;
        }}
        aria-expanded={node.isFolder ? isExpanded : undefined}
        aria-busy={isLoading ? true : undefined}
        aria-level={level}
        id={domId}
        data-testid={dataTestId}
        aria-selected={isSelected}
        aria-labelledby={`name-${domId}`}
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

          {node.isFolder && (
            <button
              onClick={() => toggle(node.id)}
              className={styles.toggleIcon}
              aria-label={isExpanded ? "minus" : "plus"}
              tabIndex={-1}
            >
              {isExpanded ? "-" : "+"}
            </button>
          )}
          {node.isFolder && (
            <button
              id={`name-${domId}`}
              className={` ${styles.folderNode} ${isSelected ? styles.selected : ""}`}
              onClick={() => handleClick(node)}
              aria-label={node.name.toLowerCase()}
              tabIndex={-1}
            >
              <div aria-hidden={true}>
                <FolderIcon />
              </div>
              {node.name}
            </button>
          )}
          {!node.isFolder && (
            <div
              id={`name-${domId}`}
              aria-label={node.name.toLowerCase()}
              className={` ${styles.fileNode}`}
            >
              <div aria-hidden={true}>
                <FileIcon />
              </div>
              {node.name}
            </div>
          )}
        </div>
        {isLoading && (
          <div className={styles.loadingIconWrapper} aria-hidden>
            <Spinner data-testid="loading-spinner" diameterPx={15} />
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
    <div className={`${className ?? ""}`}>
      <ul role="tree" aria-label="Folders" className={styles.treeList}>
        {data.map((node) => renderNode(node, 1))}
      </ul>
    </div>
  );
};

export default TreeView;
