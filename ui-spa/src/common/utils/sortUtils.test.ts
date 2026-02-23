import {
  sortByStringProperty,
  sortByDateProperty,
  sortByNumberProperty,
} from "./sortUtils";

describe("SortUtils", () => {
  describe("sortByStringProperty", () => {
    it("Should be able to sort an array of objects based on a string value property in ascending order", () => {
      const data = [
        { id: 1, name: "def" },
        { id: 2, name: "abc" },
        { id: 3, name: "lmn" },
      ];

      const expectedResult = [
        { id: 2, name: "abc" },
        { id: 1, name: "def" },
        { id: 3, name: "lmn" },
      ];

      const sortedData = sortByStringProperty(data, "name", "ascending");
      expect(sortedData).toEqual(expectedResult);
    });
    it("Should be able to sort an array of objects based on a string value property in descending order", () => {
      const data = [
        { id: 1, name: "def" },
        { id: 3, name: "lmn" },
        { id: 2, name: "abc" },
      ];

      const expectedResult = [
        { id: 3, name: "lmn" },
        { id: 1, name: "def" },
        { id: 2, name: "abc" },
      ];

      const sortedData = sortByStringProperty(data, "name", "descending");
      expect(sortedData).toEqual(expectedResult);
    });
  });

  describe("sortByDateProperty", () => {
    it("Should be able to sort an array of objects based on date value property in ascending(oldest first) order", () => {
      const data = [
        { id: 2, name: "abc", dateCreated: "2022/12/21" },
        { id: 3, name: "lmn", dateCreated: "2022/03/01" },
        { id: 1, name: "def", dateCreated: "2022/10/02" },
      ];

      const expectedResult = [
        { id: 3, name: "lmn", dateCreated: "2022/03/01" },
        { id: 1, name: "def", dateCreated: "2022/10/02" },
        { id: 2, name: "abc", dateCreated: "2022/12/21" },
      ];

      const sortedData = sortByDateProperty(data, "dateCreated", "ascending");
      expect(sortedData).toEqual(expectedResult);
    });
    it("Should be able to sort an array of objects based on date value property in descending(newest first) order", () => {
      const data = [
        { id: 3, name: "lmn", dateCreated: "2022/12/2" },
        { id: 4, name: "lmn1", dateCreated: null },
        { id: 1, name: "def", dateCreated: "2022/12/08" },
        { id: 2, name: "abc", dateCreated: "2022/12/21" },
      ];

      const expectedResult = [
        { id: 2, name: "abc", dateCreated: "2022/12/21" },
        { id: 1, name: "def", dateCreated: "2022/12/08" },
        { id: 3, name: "lmn", dateCreated: "2022/12/2" },
        { id: 4, name: "lmn1", dateCreated: null },
      ];

      const sortedData = sortByDateProperty(data, "dateCreated", "descending");
      expect(sortedData).toEqual(expectedResult);
    });
  });

  describe("sortByNumberProperty", () => {
    it("Should be able to sort an array of objects based on file size property in ascending order", () => {
      const data = [
        { id: 1, name: "file1.txt", filesize: 200 },
        { id: 2, name: "file2.txt", filesize: 100 },
        { id: 3, name: "file3.txt", filesize: 600 },
        { id: 4, name: "file4.txt", filesize: 300 },
      ];

      const expectedResult = [
        { id: 2, name: "file2.txt", filesize: 100 },
        { id: 1, name: "file1.txt", filesize: 200 },
        { id: 4, name: "file4.txt", filesize: 300 },
        { id: 3, name: "file3.txt", filesize: 600 },
      ];

      const sortedData = sortByNumberProperty(data, "filesize", "ascending");
      expect(sortedData).toEqual(expectedResult);
    });

    it("Should be able to sort an array of objects based on file size property in descending order", () => {
      const data = [
        { id: 1, name: "file1.txt", filesize: 200 },
        { id: 2, name: "file2.txt", filesize: 100 },
        { id: 3, name: "file3.txt", filesize: 600 },
        { id: 4, name: "file4.txt", filesize: 300 },
      ];

      const expectedResult = [
        { id: 3, name: "file3.txt", filesize: 600 },
        { id: 4, name: "file4.txt", filesize: 300 },
        { id: 1, name: "file1.txt", filesize: 200 },
        { id: 2, name: "file2.txt", filesize: 100 },
      ];

      const sortedData = sortByNumberProperty(data, "filesize", "descending");
      expect(sortedData).toEqual(expectedResult);
    });
  });
});
