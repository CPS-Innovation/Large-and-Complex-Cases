export const getActionDataFromId = (id: string) => {
  const lastIndex = id.lastIndexOf(":");
  if (lastIndex === -1) throw new Error("Invalid id");
  const actionData = id.slice(0, lastIndex);
  const actionType = id.slice(lastIndex + 1);
  return {
    actionData,
    actionType,
  };
};
