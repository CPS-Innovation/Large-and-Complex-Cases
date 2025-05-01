import { TabId } from "../../../common/types/CaseManagement";

type PanelProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
>;

type ItemProps = {
  id: TabId;
  label: string;
  panel: PanelProps;
};

export type CommonTabsProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> & {
  title: string;
  items: ItemProps[];
};
