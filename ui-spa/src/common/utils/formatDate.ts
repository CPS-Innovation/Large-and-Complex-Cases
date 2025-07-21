import { format, parseISO, isValid, isToday } from "date-fns";

export const formatDate = (
  dateString: string | null | undefined,
  withTime: boolean = false,
) => {
  if (!dateString) {
    return "--";
  }
  const date = parseISO(dateString);

  if (!isValid(date)) {
    return "--";
  }
  if (!withTime) {
    return isToday(date) ? "Today" : format(date, "dd/MM/yyyy");
  }
  const timeString = format(date, "h:mm aaa");
  return isToday(date)
    ? `Today, ${timeString}`
    : `${format(date, "dd/MM/yyyy")}, ${timeString}`;
};
