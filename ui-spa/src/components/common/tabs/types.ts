type PanelProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
>;

export type ItemProps<T> = {
  id: T;
  label: string;
  panel: PanelProps;
};

export type CommonTabsProps<T> = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> & {
  title: string;
  items: ItemProps<T>[];
};
