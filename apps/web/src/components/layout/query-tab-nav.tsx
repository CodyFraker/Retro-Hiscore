"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import {
  ScrollableTabItem,
  ScrollableTabList,
  TabLinkLabel,
} from "@/components/layout/scrollable-tab-list";
import { cn } from "@/lib/utils";

export type QueryTab = {
  id: string;
  label: string;
  shortLabel?: string;
};

type Props = {
  tabs: QueryTab[];
  activeTab: string;
  paramName?: string;
  ariaLabel?: string;
};

export function QueryTabNav({ tabs, activeTab, paramName = "tab", ariaLabel }: Props) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  function hrefFor(tabId: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (tabId === tabs[0]?.id) {
      params.delete(paramName);
    } else {
      params.set(paramName, tabId);
    }
    const qs = params.toString();
    return qs ? `${pathname}?${qs}` : pathname;
  }

  return (
    <ScrollableTabList ariaLabel={ariaLabel} listClassName="-mb-px">
      {tabs.map((tab) => {
        const active = tab.id === activeTab;
        return (
          <ScrollableTabItem key={tab.id}>
            <Link
              href={hrefFor(tab.id)}
              className={cn(
                "inline-block border-b-2 pb-3 text-sm font-medium whitespace-nowrap transition-colors",
                active
                  ? "border-[var(--accent-retro)] text-foreground"
                  : "border-transparent text-muted-foreground hover:border-border hover:text-foreground",
              )}
              aria-current={active ? "page" : undefined}
            >
              <TabLinkLabel label={tab.label} shortLabel={tab.shortLabel} />
            </Link>
          </ScrollableTabItem>
        );
      })}
    </ScrollableTabList>
  );
}

export function resolveQueryTab(
  raw: string | undefined,
  allowed: readonly string[],
  defaultTab: string,
): string {
  if (raw && allowed.includes(raw)) {
    return raw;
  }
  return defaultTab;
}
