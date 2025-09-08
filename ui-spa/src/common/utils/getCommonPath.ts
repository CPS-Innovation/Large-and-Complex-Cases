import commonPathPrefix from "common-path-prefix";
import { getRelativePathFromPath } from "../utils/getRelativePathFromPath";
export const getCommonPath = (filePaths: string[]) => {
  return filePaths.length === 1
    ? `${getRelativePathFromPath(filePaths[0])}/`
    : commonPathPrefix(filePaths, "/");
};
