import { useMemo } from "react";
import { useApi } from "../../../common/hooks/useApi";
import { getNetAppFolders, getCaseMetaData } from "../../../apis/gateway-api";
import TransferWidget from "../../common/transfer-widget/TransferWidget";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";

export type TransferTreeViewPageProps = {
  caseId: string;
};

const TransferTreeViewPage = ({ caseId }: TransferTreeViewPageProps) => {
  const caseMetaData = useApi(getCaseMetaData, [caseId], true);

  const initialNetappFolderData = useMemo(() => {
    if (!caseMetaData?.data) return [];
    const folders = [
      {
        id: caseMetaData.data?.netappFolderPath,
        name: getFolderNameFromPath(caseMetaData.data?.netappFolderPath),
        path: caseMetaData.data?.netappFolderPath,
        isFolder: true,
      },
    ];

    return folders;
  }, [caseMetaData]);
  return (
    <div>
      {initialNetappFolderData.length > 0 && (
        <TransferWidget
          data={initialNetappFolderData}
          onLoadChildren={async (nodeId) => {
            const data = await getNetAppFolders(nodeId);

            const folders = data.folderData.map((folder) => {
              return {
                id: folder.path,
                name: getFolderNameFromPath(folder.path),
                path: folder.path,
                isFolder: true,
              };
            });
            return folders;
          }}
          transferAction="Copy"
        />
      )}
    </div>
  );
};

export default TransferTreeViewPage;
