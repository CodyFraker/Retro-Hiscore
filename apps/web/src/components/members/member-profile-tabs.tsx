"use client";

import type { ReactNode } from "react";
import { MemberProfileHashSync } from "@/components/members/member-profile-hash-sync";
import { QueryTabNav } from "@/components/layout/query-tab-nav";

const TABS = [
  { id: "overview", label: "Overview" },
  { id: "leaderboards", label: "Leaderboards" },
  { id: "achievements", label: "Achievements" },
] as const;

type Props = {
  activeTab: string;
  panel: ReactNode;
};

export function MemberProfileTabs({ activeTab, panel }: Props) {
  return (
    <div className="space-y-8">
      <MemberProfileHashSync />
      <QueryTabNav tabs={[...TABS]} activeTab={activeTab} ariaLabel="Member profile sections" />
      <div role="tabpanel">{panel}</div>
    </div>
  );
}
