import { formatDate } from "./formatDate";
import { vi } from "vitest";
describe("formatDate", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2022-02-18T00:00:00.000Z"));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("Should format the date correctly to `ddmmyyyy` format", () => {
    expect(formatDate("2004-03-28T01:46:10.865517Z")).toEqual("28/03/2004");
    expect(formatDate("2004-10-02T13:46:10.865517Z")).toEqual("02/10/2004");
  });

  it("Should return `Today` if the date is current date is today", () => {
    expect(formatDate("2022-02-18T12:46:10.865517Z")).toEqual("Today");
    expect(formatDate("2022-02-18T20:46:10.865517Z")).toEqual("Today");
  });

  it("Should handle invalid input date", () => {
    expect(formatDate("2004-33-28")).toEqual("--");
    expect(formatDate("2004-3")).toEqual("--");
    expect(formatDate("")).toEqual("--");
    expect(formatDate(undefined)).toEqual("--");
    expect(formatDate(null)).toEqual("--");
  });

  it("Should return the date in the given format", () => {
    expect(formatDate("2022-02-18T10:46:10.865517Z", true)).toEqual(
      "Today, 10:46 am",
    );
    expect(formatDate("2022-03-15T20:30:10.865517Z", true)).toEqual(
      "15/03/2022, 8:30 pm",
    );
  });

  it("Should return the date correctly for the british summer time", () => {
    expect(formatDate("2022-07-18T10:46:10.865517Z", true)).toEqual(
      "18/07/2022, 11:46 am",
    );
    expect(formatDate("2022-08-15T20:30:10.865517Z", true)).toEqual(
      "15/08/2022, 9:30 pm",
    );
    expect(formatDate("2022-06-15T19:30:10.865517Z", true)).toEqual(
      "15/06/2022, 8:30 pm",
    );
    expect(formatDate("2022-09-15T01:30:10.865517Z", true)).toEqual(
      "15/09/2022, 2:30 am",
    );
    expect(formatDate("2022-03-15T20:30:10.865517Z", true)).toEqual(
      "15/03/2022, 8:30 pm",
    );
  });
});
