import { useState } from "react";
import { Table } from "./Table";
import ArrowDownIcon from "../../components/svgs/arrowDown.svg?react";
import TableSortIcon from "../../components/svgs/table-sort.svg?react";
import styles from "./sortableTable.module.scss";

export type SortableTableProps = {
  className?: string;
  head: (
    | {
        children: React.ReactNode;
        sortable: false;
      }
    | {
        children: React.ReactNode;
        sortable: true;
        sortName: string;
      }
  )[];
  rows: {
    cells: {
      children: React.ReactNode;
    }[];
  }[];
  handleTableSort: (sortName: string, type: "ascending" | "descending") => void;
};

export const SortableTable: React.FC<SortableTableProps> = ({
  head,
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
    if (sortState?.sortName === columnName) return sortState?.sortType;
    return "none";
  };

  const getTableHeads = () => {
    if (!head) return;
    return head.map((item) => {
      if (item.sortable) {
        return {
          children: (
            <button
              className={`${styles.sortColumnBtn}`}
              onClick={() => handleTableSortBtnClick(item.sortName)}
              aria-sort={getAriaSort(item.sortName)}
            >
              <span> {item.children}</span>
              {sortState?.sortName === item.sortName && (
                <ArrowDownIcon
                  className={
                    sortState?.sortType === "descending"
                      ? styles.descending
                      : styles.ascending
                  }
                />
              )}
              {sortState?.sortName !== item.sortName && <TableSortIcon />}
            </button>
          ),
        };
      }
      return { children: item.children };
    });
  };
  return <Table head={getTableHeads()} {...props}></Table>;
};
