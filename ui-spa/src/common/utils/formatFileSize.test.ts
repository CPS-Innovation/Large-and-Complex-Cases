import { formatFileSize } from "./formatFileSize";
describe("formatFileSize", () => {
  test("Should format correctly the filesize in KB if it is less than 1 MB", () => {
    expect(formatFileSize(10)).toStrictEqual("0.01 KB");
    expect(formatFileSize(1000)).toStrictEqual("1 KB");
    expect(formatFileSize(9000)).toStrictEqual("9 KB");
    expect(formatFileSize(900000)).toStrictEqual("900 KB");
    expect(formatFileSize(999999)).toStrictEqual("999.99 KB");
  });
  test("Should format correctly the filesize in MB if it is less than 1 MB", () => {
    expect(formatFileSize(1000000)).toStrictEqual("1 MB");
    expect(formatFileSize(1010000)).toStrictEqual("1.01 MB");
    expect(formatFileSize(9000000)).toStrictEqual("9 MB");
    expect(formatFileSize(900000000)).toStrictEqual("900 MB");
    expect(formatFileSize(999999999)).toStrictEqual("999.99 MB");
  });
  test("Should format correctly the filesize in GB if it is greater than or equal to 1GB", () => {
    expect(formatFileSize(1000000000)).toStrictEqual("1 GB");
    expect(formatFileSize(1010000000)).toStrictEqual("1.01 GB");
    expect(formatFileSize(9000000000)).toStrictEqual("9 GB");
    expect(formatFileSize(900000000000)).toStrictEqual("900 GB");
    expect(formatFileSize(999999999999)).toStrictEqual("999.99 GB");
    expect(formatFileSize(9999999999999)).toStrictEqual("9999.99 GB");
  });
});
