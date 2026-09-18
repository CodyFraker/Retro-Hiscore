import { headers } from "next/headers";
import { recordServerPageView } from "@/lib/telemetry/record-server-page-view";

export async function ServerPageViewReporter() {
  const headerList = await headers();
  const pathname = headerList.get("x-pathname");
  if (pathname) {
    recordServerPageView(pathname);
  }

  return null;
}
