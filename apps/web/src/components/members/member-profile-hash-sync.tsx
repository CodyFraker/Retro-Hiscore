"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect } from "react";
import { resolveMemberProfileTab } from "@/lib/member-profile-tab";

const HASH_TO_TAB: Record<string, string> = {
  leaderboards: "leaderboards",
  achievements: "achievements",
  overview: "overview",
};

export function MemberProfileHashSync() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  useEffect(() => {
    const hash = window.location.hash.replace(/^#/, "");
    if (!hash || !HASH_TO_TAB[hash]) {
      return;
    }
    const tab = HASH_TO_TAB[hash];
    const current = resolveMemberProfileTab(searchParams.get("tab") ?? undefined);
    if (current === tab) {
      return;
    }
    const params = new URLSearchParams(searchParams.toString());
    if (tab === "overview") {
      params.delete("tab");
    } else {
      params.set("tab", tab);
    }
    const qs = params.toString();
    router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false });
  }, [pathname, router, searchParams]);

  return null;
}
