import { recordPageView } from "@/lib/telemetry/platform-metrics";
import { initNodeMetrics } from "@/lib/telemetry/init-node-metrics";

export function recordServerPageView(pathname: string) {
  initNodeMetrics();
  recordPageView(pathname, "server");
}
