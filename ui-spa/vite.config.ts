/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";
import istanbul from "vite-plugin-istanbul";

export default defineConfig(({ command, mode }) => {
  const isE2ECoverage = process.env.E2E_COVERAGE === "1";
  const isProdBuild = command === "build" && mode === "production";
  const buildSourceMap = isE2ECoverage || !isProdBuild;

  return {
    build: { sourcemap: buildSourceMap },
    plugins: [
      react(), 
      svgr(),
      isE2ECoverage &&
        istanbul({
          include: ["src/**/*"],
          exclude: [
            "src/**/*.{test,spec}.{ts,tsx}",
            "src/tests/*",
            "src/mocks",
            "src/common/types",
            "src/components",
            "src/auth/mock",
            "src/auth/no-auth",
            "src/auth/index.ts",
            "src/auth/userDetails.ts",
            "src/config.ts",
            "src/types.d.ts",
            "src/vite-env.d.ts",
            "src/main.tsx",
          ],
          requireEnv: false,
          forceBuildInstrument: true
        }),
    ].filter(Boolean),
    test: {
      globals: true,
      environment: "jsdom",
      setupFiles: ["./src/tests/setup.ts"],
      include: ["src/**/*.{test,spec}.{ts,tsx}"],
      reporters: ["default", "junit"],
      outputFile: {
        junit: "./coverage/unit-test-results.xml",
      },
      coverage: {
        enabled: true,
        reporter: ["json"],
        provider: "v8",
        reportsDirectory: "coverage/reports",
        include: ["src/*"],
        exclude: [
          "src/mocks",
          "src/common/types",
          "src/components",
          "src/auth/mock",
          "src/auth/no-auth",
          "src/auth/index.ts",
          "src/auth/userDetails.ts",
          "src/config.ts",
          "src/types.d.ts",
          "src/vite-env.d.ts",
          "src/main.tsx",
        ],
      },
    },
    server: {
      port: 5173,
      strictPort: true,
    },
    preview: {
      port: 5173,
      strictPort: true,
    },
  };
});
