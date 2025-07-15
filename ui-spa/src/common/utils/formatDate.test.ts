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
    expect(formatDate("2004-03-28")).toEqual("28/03/2004");
    expect(formatDate("2004-10-02")).toEqual("02/10/2004");
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
});
