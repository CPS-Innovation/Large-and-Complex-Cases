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
    coverage: {
      enabled: true,
      reporter: ["text", "json", "html"],
      provider: "v8",
      include: ["src/*"],
      exclude: [
        "src/mocks",
        "src/common/types",
        "src/components",
        "src/auth/mock",
        "src/auth/no-auth",
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
