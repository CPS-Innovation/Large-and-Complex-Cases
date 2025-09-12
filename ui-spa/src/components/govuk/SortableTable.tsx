import { useState } from "react";
import { Table } from "./Table";
import ArrowDownIcon from "../../components/svgs/arrowDown.svg?react";
import TableSortIcon from "../../components/svgs/table-sort.svg?react";
import styles from "./SortableTable.module.scss";

export type SortableTableProps = {
  className?: string;
  caption: string;
  captionClassName: string;
  head: {
    children: React.ReactNode;
    sortable: boolean;
    sortName?: string;
  }[];
  rows: {
    cells: {
      children: React.ReactNode;
    }[];
  }[];
  handleTableSort: (sortName: string, type: "ascending" | "descending") => void;
};

export const SortableTable: React.FC<SortableTableProps> = ({
  head,
  caption,
  captionClassName,
  handleTableSort,
  ...props
}) => {
  const [sortState, setSortState] = useState<{
    sortName: string;
    sortType: "ascending" | "descending";
  }>();

  const handleTableSortBtnClick = (columnName: string) => {
    let sortType: "ascending" | "descending" = "ascending";
    if (
      sortState?.sortName === columnName &&
      sortState?.sortType === "ascending"
    ) {
      sortType = "descending";
    }

    setSortState({
      sortName: columnName,
      sortType: sortType,
    });
    handleTableSort(columnName, sortType);
  };

  const getAriaSort = (columnName: string) => {
    if (sortState?.sortName === columnName)
      return `${sortState?.sortType}` as const;
    return "none" as const;
  };

  const getTableHeads = () => {
    // if (!head) return;
    return head.map((item) => {
      if (item.sortName) {
        return {
          ariaSort: getAriaSort(item.sortName),
          children: (
            <button
              className={`${styles.sortColumnBtn}`}
              onClick={() => handleTableSortBtnClick(item.sortName!)}
            >
              <span> {item.children}</span>
              {sortState?.sortName === item.sortName && (
                <ArrowDownIcon
                  data-testid={
                    sortState?.sortType === "descending"
                      ? "arrow-down-icon"
                      : "arrow-up-icon"
                  }
                  className={
                    sortState?.sortType === "descending"
                      ? styles.descending
                      : styles.ascending
                  }
                />
              )}
              {sortState?.sortName !== item.sortName && (
                <TableSortIcon data-testid="sort-icon" />
              )}
            </button>
          ),
        };
      }
      return { children: item.children };
    });
  };
  return (
    <Table
      caption={caption}
      captionClassName={captionClassName}
      head={getTableHeads()}
      {...props}
    ></Table>
  );
};
