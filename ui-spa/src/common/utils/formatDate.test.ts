import { formatDate } from "./formatDate";

describe("formatDate", () => {
  it("Should format the date correctly to `ddmmyyyy` format", () => {
    expect(formatDate("2004-03-28")).toEqual("28/03/2004");
    expect(formatDate("2004-10-02")).toEqual("02/10/2004");
  });

  it("Should handle invalid input date", () => {
    expect(formatDate("2004-33-28")).toEqual("invalid date");
    expect(formatDate("2004-3")).toEqual("invalid date");
    expect(formatDate("")).toEqual("invalid date");
  });
});
