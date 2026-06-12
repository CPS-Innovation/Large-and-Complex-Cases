import { BackLink } from "../../govuk";
import { useState, useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import {
  type EgressFolderData,
  type NetAppFolderData,
  type NetAppFolder,
} from "../../../schemas";
import { getNetAppFolders, getEgressFolders } from "../../../apis/gateway-api";
import { InitiateFileTransferPayload } from "../../../schemas/requests/initiateFileTransferPayload";
import { IndexingFileTransferPayload } from "../../../schemas/requests/indexingFileTransferPayload";
import type {
  IndexingFileTransferResponse,
  InitiateFileTransferResponse,
} from "../../../schemas";
import {
  type EgressTransferPayloadSourcePath,
  type NetAppTransferPayloadSourcePath,
} from "../../../schemas/requests/initiateFileTransferPayload";
import { getCommonPath } from "../../../common/utils/getCommonPath";
import {
  indexingFileTransfer,
  initiateFileTransfer,
} from "../../../apis/gateway-api";
import TransferWidget from "../../common/transfer-widget/TransferWidget";
import { useMutation, useQuery } from "@tanstack/react-query";
import { getFolderNameFromPath } from "../../../common/utils/getFolderNameFromPath";
import { ApiError } from "../../../common/errors/ApiError";
import { type TreeNode } from "../../common/tree-view-component/TreeViewComponent";

const TransferDestinationPage: React.FC = () => {
  const {
    state: {
      transferSource,
      sourcePaths,
      caseId,
      egressWorkspaceId,
      selectedTransferAction,
      netAppFolderPath,
      operationName,
    },
  }: {
    state: {
      sourcePaths:
        | {
            fileId: string;
            path: string;
            isFolder: boolean;
          }[]
        | { path: string }[];
      transferSource: "egress" | "netapp";
      egressWorkspaceId: string;
      selectedTransferAction: "copy" | "move";
      caseId: number;
      netAppFolderPath: string;
      operationName: string;
    };
  } = useLocation();

  const {
    data: netAppData,
    refetch: netAppRefetch,
    isLoading: isNetAppFolderDataLoading,
  } = useQuery({
    queryKey: [netAppFolderPath],
    queryFn: () => getNetAppFolders(netAppFolderPath),
    retry: false,
    enabled: transferSource === "egress",
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
  });

  const getNetAppTreeViewData = (folderData: NetAppFolder[]): TreeNode[] => {
    const folders = folderData.map((folder) => {
      return {
        id: folder.path,
        name: getFolderNameFromPath(folder.path),
        path: folder.path,
        isFolder: true,
        isRootNode: false,
      };
    });

    return folders;
  };
  const initialNetAppFolderData = useMemo(() => {
    const folders = [
      {
        id: netAppFolderPath,
        name: `Shared drive: ${getFolderNameFromPath(netAppFolderPath)}`,
        path: netAppFolderPath,
        isFolder: true,
        isRootNode: true,
        children: netAppData?.folderData
          ? getNetAppTreeViewData(netAppData?.folderData)
          : [],
      },
    ];

    return folders;
  }, [netAppFolderPath, netAppData]);

  const initialEgressFolderData = useMemo(() => {
    const folders = [
      {
        id: "root",
        name: `Egress : ${operationName}`,
        path: "",
        isFolder: true,
      },
    ];

    return folders;
  }, [operationName]);

  const [transferStatus, setTransferStatus] = useState<
    "validating" | "transferring" | null
  >(null);

  const initiateFileTransferMutation = useMutation({
    mutationFn: initiateFileTransfer,
    throwOnError: (error: ApiError) => {
      return error.code !== 403;
    },
  });

  const indexingFileTransferMutation = useMutation({
    mutationFn: indexingFileTransfer,
    throwOnError: true,
  });

  const navigate = useNavigate();

  const getIndexingFileTransferPayload = (
    destinationPath: string,
  ): IndexingFileTransferPayload => {
    const paths = sourcePaths.map(({ path }) => path);

    const payload = {
      caseId: caseId,
      transferDirection:
        transferSource === "egress"
          ? ("EgressToNetApp" as const)
          : ("NetAppToEgress" as const),
      transferType:
        selectedTransferAction === "copy"
          ? ("Copy" as const)
          : ("Move" as const),
      sourcePaths: sourcePaths,
      destinationPath: destinationPath,
      workspaceId: egressWorkspaceId,
      sourceRootFolderPath: getCommonPath(paths),
    };
    return payload;
  };

  const handleInitiateFileTransfer = async (
    initiatePayload: InitiateFileTransferPayload,
  ) => {
    setTransferStatus("transferring");

    const initiateFileTransferResponse: InitiateFileTransferResponse =
      await initiateFileTransferMutation.mutateAsync(initiatePayload);

    navigate(`/case/${caseId}/case-management`, {
      replace: true,
      state: {
        transferId: initiateFileTransferResponse.id,
      },
    });
  };

  const getInitiateTransferPayload = (
    response: IndexingFileTransferResponse,
    egressWorkspaceId: string,
    caseId: number,
  ): InitiateFileTransferPayload => {
    let sourcePaths:
      | EgressTransferPayloadSourcePath[]
      | NetAppTransferPayloadSourcePath[];
    const transferDirection: "EgressToNetApp" | "NetAppToEgress" =
      response.transferDirection === "EgressToNetApp"
        ? ("EgressToNetApp" as const)
        : ("NetAppToEgress" as const);
    if (response.transferDirection === "EgressToNetApp") {
      sourcePaths = response.files.map((data) => ({
        fileId: data.id ?? undefined,
        path: data.sourcePath,
        fullFilePath: data.fullFilePath ?? undefined,
      }));
    } else {
      sourcePaths = response.files.map((data) => ({
        path: data.sourcePath,
        relativePath: data.relativePath ?? undefined,
      }));
    }

    const payload = {
      caseId: caseId,
      destinationPath: response.destinationPath,
      workspaceId: egressWorkspaceId,
      sourceRootFolderPath: response.sourceRootFolderPath,
      sourcePaths: sourcePaths,
      transferType:
        selectedTransferAction === "copy"
          ? ("Copy" as const)
          : ("Move" as const),
      transferDirection: transferDirection,
    } as InitiateFileTransferPayload;

    return payload;
  };

  const handleValidateTransfer = async (destinationPath: string) => {
    const validationPayload = getIndexingFileTransferPayload(destinationPath);
    try {
      setTransferStatus("validating");
      const response: IndexingFileTransferResponse =
        await indexingFileTransferMutation.mutateAsync(validationPayload);
      if (response.isInvalid) {
        navigate(`/case/${caseId}/case-management/transfer-resolve-file-path`, {
          state: {
            isRouteValid: true,
            validationErrors: response.validationErrors,
            destinationPath: response.destinationPath,
            initiateTransferPayload: getInitiateTransferPayload(
              response,
              egressWorkspaceId,
              caseId,
            ),
            baseFolderName: validationPayload.sourceRootFolderPath,
          },
        });
        return;
      }

      const initiateTransferPayload = getInitiateTransferPayload(
        response,
        egressWorkspaceId,
        caseId,
      );
      handleInitiateFileTransfer(initiateTransferPayload);
    } catch (error) {
      if (
        error instanceof ApiError &&
        error.code == 403 &&
        validationPayload.transferType === "Move"
      ) {
        navigate(`/case/${caseId}/case-management/transfer-permissions-error`, {
          state: {
            isRouteValid: true,
          },
        });
        return;
      }
      return;
    }
  };

  const handleLoadChildren = async (nodeId: string) => {
    if (transferSource === "egress") {
      const data = await getNetAppFolders(nodeId);
      const folders = data.folderData.map((folder) => {
        return {
          id: folder.path,
          name: getFolderNameFromPath(folder.path),
          path: folder.path,
          isFolder: true,
          isRootNode: true,
        };
      });

      return folders;
    } else {
      const newId = nodeId === "root" ? "" : nodeId;
      const folderData = await getEgressFolders(egressWorkspaceId, newId);
      const folders = folderData
        .filter((data) => data.isFolder)
        .map((data) => {
          return {
            id: data.id,
            name: data.name,
            path: data.path ? `${data.path}/${data.name}/` : `${data.name}/`,
            isFolder: data.isFolder,
          };
        });
      return folders;
    }
  };

  const handleTransfer = (selectedNode: TreeNode) => {
    console.log("Selected node for transfer:", selectedNode.path);
    handleValidateTransfer(selectedNode.path);
  };

  return (
    <div>
      <BackLink to={`/case/${caseId}/case-management`} replace>
        Back
      </BackLink>
      <PageContentWrapper>
        <div>
          <h1>
            {transferSource === "egress"
              ? "Choose a Shared Drive folder"
              : "Choose an Egress folder"}
          </h1>
          <p>
            {sourcePaths.length === 1
              ? "You are copying 1 item."
              : `You are copying ${sourcePaths.length} items.`}
          </p>
          <p>
            {transferSource === "egress"
              ? "Select the Shared Drive folder you want to copy them into."
              : "Select the Egress folder you want to copy them into."}
          </p>
        </div>

        <div>
          <TransferWidget
            data={
              transferSource === "egress"
                ? initialNetAppFolderData
                : initialEgressFolderData
            }
            transferAction={selectedTransferAction === "copy" ? "Copy" : "Move"}
            onLoadChildren={handleLoadChildren}
            handleTransfer={handleTransfer}
          />
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default TransferDestinationPage;
