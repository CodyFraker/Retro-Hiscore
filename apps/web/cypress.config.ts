import { defineConfig } from "cypress";
import { spawn, type ChildProcess } from "node:child_process";
import path from "node:path";

let mockApi: ChildProcess | null = null;

export default defineConfig({
  e2e: {
    baseUrl: "http://localhost:18321",
    supportFile: false,
    specPattern: "cypress/e2e/**/*.cy.ts",
    video: false,
    setupNodeEvents(on) {
      on("before:run", async () => {
        mockApi = spawn("node", [path.resolve(__dirname, "../../scripts/mock-api.mjs")], {
          env: { ...process.env, MOCK_API_PORT: "18943" },
          stdio: "inherit",
        });
      });
      on("after:run", async () => {
        mockApi?.kill();
      });
    },
  },
});
