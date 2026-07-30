import { MoveSoakScenario } from "../helpers/types";


export const moveSoakScenarios: MoveSoakScenario[] = [
  {
    name: "10 x 2GB - one full batch (peak parallel fan-out)",
    specs: [{ fileSizeMb: 2048, fileCount: 10 }],
    timeout: 45 * 60 * 1000,
  },
  {
    name: "11 x 2GB - crosses the batch boundary into a second batch",
    specs: [{ fileSizeMb: 2048, fileCount: 11 }],
    timeout: 60 * 60 * 1000,
  },
  {
    name: "Mixed batch - 5 x 2GB + 5 x 5MB share one batch",
    specs: [
      { fileSizeMb: 2048, fileCount: 5 },
      { fileSizeMb: 5, fileCount: 5 },
    ],
    timeout: 45 * 60 * 1000,
  },
];


