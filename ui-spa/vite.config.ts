/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";

export default defineConfig({
  plugins: [react(), svgr()],
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
      reporter: ["text", "json", "html", "cobertura"],
      provider: "v8",
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
});
