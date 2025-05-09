import * as GDS from "govuk-react-jsx";

export type InsetTextProps = {
  className?: string;
  children?: React.ReactNode;
};

export const InsetText: React.FC<InsetTextProps> = (props) => {
  return <GDS.InsetText {...props}></GDS.InsetText>;
};
