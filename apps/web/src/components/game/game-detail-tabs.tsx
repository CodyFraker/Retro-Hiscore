"use client";

import type { ReactNode } from "react";
import { useSearchParams } from "next/navigation";
import { QueryTabNav, resolveQueryTab } from "@/components/layout/query-tab-nav";

const TABS = [
  { id: "standings", label: "Standings" },
  { id: "trends", label: "Trends" },
  { id: "achievements", label: "Achievements" },
] as const;

const TAB_IDS = TABS.map((t) => t.id);

type Props = {
  standings: ReactNode;
  trends: ReactNode;
  achievements: ReactNode;
};

export function GameDetailTabs({ standings, trends, achievements }: Props) {
  const searchParams = useSearchParams();
  const activeTab = resolveQueryTab(searchParams.get("tab") ?? undefined, TAB_IDS, "standings");

  return (
    <div className="space-y-8">
      <QueryTabNav tabs={[...TABS]} activeTab={activeTab} ariaLabel="Game sections" />
      <div hidden={activeTab !== "standings"}>{standings}</div>
      <div hidden={activeTab !== "trends"}>{trends}</div>
      <div hidden={activeTab !== "achievements"}>{achievements}</div>
    </div>
  );
}
