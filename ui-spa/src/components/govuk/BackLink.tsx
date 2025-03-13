import React, { ReactNode } from "react";
import * as GDS from "govuk-react-jsx";

export type BackLinkProps = {
  href?: string;
  to?: string;
  label?: ReactNode;
  onClick?: () => void;
  children: ReactNode;
};

export const BackLink: React.FC<BackLinkProps> = (props) => {
  return <GDS.BackLink data-testid="link-back-link" {...props} />;
};
