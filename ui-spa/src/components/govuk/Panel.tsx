import * as GDS from "govuk-react-jsx";
import React from "react";

type Props = {
  titleChildren: React.ReactNode;
  headingLevel?: 1 | 2 | 3 | 4 | 5 | 6;
  children?: React.ReactNode;
};
export const Panel: React.FC<Props> = (props) => <GDS.Panel {...props} />;
