import * as GDS from "govuk-react-jsx";

type Props = {
  className?: string;
  children: React.ReactNode;
  type?: "success";
};
export const NotificationBanner: React.FC<Props> = (props) => (
  <GDS.NotificationBanner {...props} />
);
