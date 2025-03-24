const { format, parseISO, isValid } = require("date-fns");

export const formatDate = (dateString: string) => {
  const date = parseISO(dateString);

  if (!isValid(date)) {
    return "invalid date";
  }
  return format(date, "dd/MM/yyyy");
};
