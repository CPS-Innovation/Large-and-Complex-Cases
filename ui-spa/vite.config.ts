/// <reference types="vitest" />
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";
import csp from "vite-plugin-csp-guard";
import istanbul from "vite-plugin-istanbul";

export default defineConfig(({ command, mode }) => {
  const isE2ECoverage = process.env.E2E_COVERAGE === "1";
  const isProdBuild = command === "build" && mode === "production";
  const buildSourceMap = isE2ECoverage || !isProdBuild;
  const env = loadEnv(mode, process.cwd(), "");

  const getAllowedSources = () => {
    if (env.VITE_FEATURE_FLAG_GLOBAL_NAV !== "true") {
      return [];
    }
    if (
      env.VITE_GLOBAL_NAV_SCRIPT_URL?.includes(
        "https://sacpsglobalcomponents.blob.core.windows.net/",
      )
    ) {
      return [
        "https://sacpsglobalcomponents.blob.core.windows.net/",
        "https://polaris-qa-notprod.cps.gov.uk/",
      ];
    }
    if (
      env.VITE_GLOBAL_NAV_SCRIPT_URL?.includes(
        "https://polaris-qa-notprod.cps.gov.uk/",
      )
    ) {
      return ["https://polaris-qa-notprod.cps.gov.uk/"];
    }
    if (
      env.VITE_GLOBAL_NAV_SCRIPT_URL?.includes("https://polaris.cps.gov.uk/")
    ) {
      return ["https://polaris.cps.gov.uk/"];
    }
    return [];
  };
  return {
    build: { sourcemap: buildSourceMap },
    plugins: [
      react(),
      svgr(),
      {
        name: "inject-external-script",
        transformIndexHtml(html: string) {
          if (
            env.VITE_GLOBAL_NAV_SCRIPT_URL &&
            env.VITE_FEATURE_FLAG_GLOBAL_NAV === "true"
          ) {
            return html.replace(
              "</head>",
              `<script src="${env.VITE_GLOBAL_NAV_SCRIPT_URL}" type="module"></script>\n</head>`,
            );
          }
          return html;
        },
      },
      csp({
        dev: {
          run: true,
        },
        policy: {
          "default-src": ["'self'"],
          "script-src": ["'self'"],
          "script-src-elem": ["'self'", ...getAllowedSources()],
          "connect-src": [
            env.VITE_GATEWAY_BASE_URL ?? "",
            ...getAllowedSources(),
            "https://login.microsoftonline.com",
            "https://js.monitor.azure.com/",
          ],
          "style-src-elem": ["'self'", "'unsafe-inline'"],
          "img-src": ["'self'", "data:"],
          "font-src": ["'self'"],
        },
      }),
      isE2ECoverage &&
        istanbul({
          include: ["src/**/*.{ts,tsx,js,jsx}"],
          exclude: [
            "src/**/*.{test,spec}.ts",
            "src/**/*.{test,spec}.tsx",
            "src/tests/*",
            "src/mocks",
            "src/common/types",
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
          forceBuildInstrument: true,
        }),
    ].filter(Boolean),
    test: {
      globals: true,
      environment: "jsdom",
      setupFiles: ["./src/tests/setup.ts"],
      include: ["src/**/*.{test,spec}.{ts,tsx}"],
      reporters: ["default", "junit"],
      outputFile: {
        junit: "./unit-test-results.xml",
      },
      coverage: {
        enabled: true,
        reporter: ["text", "json", "cobertura"],
        provider: "v8",
        reportsDirectory: "coverage/unit",
        include: ["src/**/*.{ts,tsx,js,jsx}"],
        exclude: [
          "src/**/*.{test,spec}.ts",
          "src/**/*.{test,spec}.tsx",
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
