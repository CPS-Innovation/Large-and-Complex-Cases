import { MoveSoakScenario } from "../helpers/types";

export const moveSoakScenarios: MoveSoakScenario[] = [
  // {
  //   name: "1 x 2GB - single multipart upload path",
  //   specs: [{ fileSizeMb: 2048, fileCount: 1 }],
  // },
  // {
  //   name: "10 x 2GB - peak batch parallelism",
  //   specs: [{ fileSizeMb: 2048, fileCount: 10 }],
  //   timeout: 45 * 60 * 1000,
  // },
  // {
  //   name: "11 x 2GB - crosses batch boundary",
  //   specs: [{ fileSizeMb: 2048, fileCount: 11 }],
  //   timeout: 60 * 60 * 1000,
  // },
  // {
  //   name: "100 x 2GB - full load (10 batches)",
  //   specs: [{ fileSizeMb: 2048, fileCount: 100 }],
  //   timeout: 90 * 60 * 1000
  // },
  {
    name: "2GB + injected failure (retry validation)",
    specs: [{ fileSizeMb: 2048, fileCount: 5}],
    injectFailure: true,
    timeout: 60 * 60 * 1000
  },
  // {
  //   name: "Mixed batch (2GB + small files)",
  //   specs: [
  //     { fileSizeMb: 2048, fileCount: 10 },
  //     { fileSizeMb: 5, fileCount: 5 },
  //   ],
  //   timeout: 60 * 60 * 1000,
  // },
] as const;
