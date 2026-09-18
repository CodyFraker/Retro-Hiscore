"use client";

import { usePathname } from "next/navigation";
import { useEffect, useRef } from "react";
import { recordPageViewAction } from "@/lib/actions/telemetry";

export function RouteViewReporter() {
  const pathname = usePathname();
  const lastRecorded = useRef<string | null>(null);

  useEffect(() => {
    if (!pathname || lastRecorded.current === pathname) {
      return;
    }

    lastRecorded.current = pathname;
    void recordPageViewAction(pathname);
  }, [pathname]);

  return null;
}
