import { format, parseISO, isValid, isToday } from "date-fns";
import { tz } from "@date-fns/tz";

export const formatDate = (
  dateString: string | null | undefined,
  withTime: boolean = false,
) => {
  const londonTime = tz("Europe/London");
  if (!dateString) {
    return "--";
  }
  const date = parseISO(dateString);

  if (!isValid(date)) {
    return "--";
  }
  const formattedDate = format(date, "dd/MM/yyyy", { in: londonTime });
  if (!withTime) {
    return isToday(date) ? "Today" : formattedDate;
  }
  const timeString = format(date, "h:mm aaa", { in: londonTime });
  return isToday(date)
    ? `Today, ${timeString}`
    : `${formattedDate}, ${timeString}`;
};
