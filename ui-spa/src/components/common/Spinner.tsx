import classes from "./Spinner.module.scss";

type Props = {
  diameterPx: number;
  ariaLabel: string;
};

export const Spinner: React.FC<Props> = ({
  diameterPx,
  ariaLabel,
  ...props
}) => (
  <div
    role="status"
    aria-live="polite"
    className={`${classes.spinner} spinner`}
    style={{ height: diameterPx, width: diameterPx }}
    aria-label={ariaLabel}
    {...props}
  ></div>
);
