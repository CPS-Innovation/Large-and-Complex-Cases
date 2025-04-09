import { format, parseISO, isValid } from "date-fns";

export const formatDate = (dateString: string) => {
  if(!dateString) {
    return null;
  }
  const date = parseISO(dateString);

  if (!isValid(date)) {
    return "invalid date";
  }
  return format(date, "dd/MM/yyyy");
};
