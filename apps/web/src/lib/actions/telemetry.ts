"use server";

import { recordPageView } from "@/lib/telemetry/platform-metrics";
import { initNodeMetrics } from "@/lib/telemetry/init-node-metrics";

export async function recordPageViewAction(pathname: string) {
  initNodeMetrics();
  recordPageView(pathname, "client");
}
