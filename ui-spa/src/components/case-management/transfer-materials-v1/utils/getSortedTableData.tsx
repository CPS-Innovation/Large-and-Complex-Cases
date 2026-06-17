import {
  sortByStringProperty,
  sortByDateProperty,
  sortByNumberProperty,
} from "../../../../common/utils/sortUtils";
import {
  type EgressFolderData,
  type NetAppFolderData,
} from "../../../../schemas";
import { isEgressFolderDetails } from "./isEgressFolderData";

export const getSortedTableData = (
  data: EgressFolderData | NetAppFolderData,
  sortValues: { name: string; type: "ascending" | "descending" },
) => {
  if (sortValues.name === "folder-name") {
    if (isEgressFolderDetails(data)) {
      return sortByStringProperty(data, "name", sortValues.type);
    }

    return sortByStringProperty(data, "path", sortValues.type);
  }

  if (sortValues.name === "date-updated") {
    if (isEgressFolderDetails(data)) {
      return sortByDateProperty(data, "dateUpdated", sortValues.type);
    }

    return sortByDateProperty(data, "lastModified", sortValues.type);
  }

  if (sortValues.name === "file-size") {
    if (isEgressFolderDetails(data)) {
      return sortByNumberProperty(data, "filesize", sortValues.type);
    }

    return sortByNumberProperty(data, "filesize", sortValues.type);
  }

  return data;
};
