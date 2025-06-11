import { getActionDataFromId } from "./getActionDataFromId";
describe("getActionDataFromId", () => {
  test("it should return the action data and action type from the given string", () => {
    expect(getActionDataFromId("path1234:move")).toEqual({
      actionData: "path1234",
      actionType: "move",
    });
  });
  test("it should throw invalid id error if the given string is not in correct format", () => {
    expect(() => getActionDataFromId("path1234move")).toThrowError(
      "Invalid id",
    );
  });
  test("it should only consider last occurance of ':' character to split the string", () => {
    expect(getActionDataFromId("path:1234:move")).toEqual({
      actionData: "path:1234",
      actionType: "move",
    });
  });
});
