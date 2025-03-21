import { mapAreaLookups } from "./mapAreaLookups";
describe("mapAreaLookups", () => {
  it("Should sort arealooksup data alphabetically in the ascending order", () => {
    const inputData = {
      allAreas: [
        {
          id: 2,
          description: "Mabc",
        },
        {
          id: 5,
          description: "dbc",
        },
        {
          id: 6,
          description: "Rbc",
        },
        {
          id: 7,
          description: "Rbc",
        },
      ],
      userAreas: [
        {
          id: 1057709,
          description: "abc",
        },
        {
          id: 1057708,
          description: "Mabc",
        },
      ],
      homeArea: {
        id: 1057709,
        description: "abc",
      },
    };
    const expectedResult = {
      allAreas: [
        {
          id: 5,
          description: "dbc",
        },
        {
          id: 2,
          description: "Mabc",
        },
        {
          id: 6,
          description: "Rbc",
        },
        {
          id: 7,
          description: "Rbc",
        },
      ],
      userAreas: [
        {
          id: 1057709,
          description: "abc",
        },
        {
          id: 1057708,
          description: "Mabc",
        },
      ],
      homeArea: {
        id: 1057709,
        description: "abc",
      },
    };

    const result = mapAreaLookups(inputData);
    expect(result).toStrictEqual(expectedResult);
  });
});
