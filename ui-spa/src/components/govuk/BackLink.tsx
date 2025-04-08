import React, { ReactNode } from "react";
import * as GDS from "govuk-react-jsx";
import { Link } from "react-router";

export type BackLinkProps = {
  href?: string;
  to?: string;
  state?: any;
  children: ReactNode;
};

export const BackLink: React.FC<BackLinkProps> = (props) => {
  //This is overwrite the original Link component implementation based on older version of react-router which throws error
  if (props.to)
    return (
      <Link
        to={props.to}
        className="govuk-back-link"
        data-testid="link-back-link"
        {...props}
      >
        {props.children}
      </Link>
    );
  return <GDS.BackLink data-testid="link-back-link" {...props} />;
};
