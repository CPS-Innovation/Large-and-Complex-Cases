import { Details as GDSDetails } from "govuk-react-jsx";
export type DetailsProps = React.DetailedHTMLProps<
  React.DetailsHTMLAttributes<HTMLDetailsElement>,
  HTMLDetailsElement
> & {
  className?: string;
  children?: React.ReactNode;
  summaryChildren?: React.ReactNode;
};

export const Details: React.FC<DetailsProps> = ({ ...restProps }) => {
  return <GDSDetails {...restProps}></GDSDetails>;
};
