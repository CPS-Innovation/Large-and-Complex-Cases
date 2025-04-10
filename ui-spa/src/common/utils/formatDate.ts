import { format, parseISO, isValid } from "date-fns";

export const formatDate = (dateString: string | null | undefined) => {
  if (!dateString) {
    return "--";
  }
  const date = parseISO(dateString);

  if (!isValid(date)) {
    return "--";
  }
  return format(date, "dd/MM/yyyy");
};
