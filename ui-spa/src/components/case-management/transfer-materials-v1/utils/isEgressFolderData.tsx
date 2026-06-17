import {
  type EgressFolderData,
  type NetAppFolderData,
  egressFolderDataSchema,
  netAppFolderDataSchema,
} from "../../../../schemas";

export function isEgressFolderDetails(
  details: EgressFolderData | NetAppFolderData,
): details is EgressFolderData {
  return egressFolderDataSchema.safeParse(details).success;
}

export function isNetAppFolderDetails(
  details: EgressFolderData | NetAppFolderData,
): details is NetAppFolderData {
  return netAppFolderDataSchema.safeParse(details).success;
}
