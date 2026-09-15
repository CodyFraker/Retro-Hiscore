"use client";

import type { ReactNode } from "react";
import { useSearchParams } from "next/navigation";
import { QueryTabNav, resolveQueryTab } from "@/components/layout/query-tab-nav";

const TABS = [
  { id: "overview", label: "Overview" },
  { id: "leaderboards", label: "Leaderboards" },
  { id: "achievements", label: "Achievements" },
] as const;

const TAB_IDS = TABS.map((t) => t.id);

type Props = {
  overview: ReactNode;
  leaderboards: ReactNode;
  achievements: ReactNode;
};

export function MemberProfileTabs({ overview, leaderboards, achievements }: Props) {
  const searchParams = useSearchParams();
  const activeTab = resolveQueryTab(searchParams.get("tab") ?? undefined, TAB_IDS, "overview");

  return (
    <div className="space-y-8">
      <QueryTabNav tabs={[...TABS]} activeTab={activeTab} ariaLabel="Member profile sections" />
      <div hidden={activeTab !== "overview"}>{overview}</div>
      <div hidden={activeTab !== "leaderboards"}>{leaderboards}</div>
      <div hidden={activeTab !== "achievements"}>{achievements}</div>
    </div>
  );
}
