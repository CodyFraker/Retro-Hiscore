"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { cn } from "@/lib/utils";

export type QueryTab = {
  id: string;
  label: string;
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
    <nav className="border-b border-border" aria-label={ariaLabel}>
      <ul className="-mb-px flex gap-4 overflow-x-auto sm:gap-6">
        {tabs.map((tab) => {
          const active = tab.id === activeTab;
          return (
            <li key={tab.id} className="shrink-0">
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
                {tab.label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
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
