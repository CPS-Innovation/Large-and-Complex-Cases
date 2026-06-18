import Checkbox from "../../components/common/Checkbox";
export const getTransferSourceTableHeadData = (
  handleCheckboxChange: (id: string, checked: boolean) => void,
  isSourceFolderChecked: (checkboxId: string) => boolean,
) => {
  return [
    {
      children: (
        <Checkbox
          id={"all-folders"}
          checked={isSourceFolderChecked("all-folders")}
          onChange={handleCheckboxChange}
          ariaLabel="Select folders and files"
        />
      ),
      sortable: false,
    },
    {
      children: <>Folder/file name</>,
      sortable: true,
      sortName: "folder-name",
    },
    {
      children: <>Last modified date</>,
      sortable: true,
      sortName: "date-updated",
    },
    {
      children: <>Size</>,
      sortable: true,
      sortName: "file-size",
    },
  ];
};
