export async function register() {
  if (process.env.NEXT_RUNTIME === "nodejs") {
    const { initNodeMetrics } = await import("@/lib/telemetry/init-node-metrics");
    initNodeMetrics();
  }
}
