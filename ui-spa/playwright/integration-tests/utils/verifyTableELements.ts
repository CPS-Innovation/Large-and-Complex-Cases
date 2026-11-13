import { type Page, expect } from "@playwright/test";

export const verifyTableHeaders = async (
  page: Page,
  expectedHeaders: string[],
) => {
  const headers = page.locator("table thead th");
  await expect(headers).toHaveCount(expectedHeaders.length);

  await Promise.all(
    expectedHeaders.map((header, i) =>
      expect(headers.nth(i)).toHaveText(header),
    ),
  );
};

export const verifyTableRows = async (page: Page, expectedRows: string[][]) => {
  const rows = page.locator("table tbody tr");
  await expect(rows).toHaveCount(expectedRows.length);

  await Promise.all(
    expectedRows.map((expectedCells, i) => {
      const cells = rows.nth(i).locator("td");
      return Promise.all([
        expect(cells).toHaveCount(expectedCells.length),
        ...expectedCells.map((cell, j) =>
          expect(cells.nth(j)).toHaveText(cell),
        ),
      ]);
    }),
  );
};
