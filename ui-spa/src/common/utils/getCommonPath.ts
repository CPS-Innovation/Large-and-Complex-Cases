import commonPathPrefix from "common-path-prefix";
import { getRelativePathFromPath } from "../utils/getRelativePathFromPath";

export const getCommonPath = (filePaths: string[]) => {
  if (filePaths.length === 1) {
    if (filePaths[0] === "") return "";
    return `${getRelativePathFromPath(filePaths[0])}/`;
  }
  return commonPathPrefix(filePaths, "/");
};
