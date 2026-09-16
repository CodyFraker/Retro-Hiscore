"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  ScrollableTabItem,
  ScrollableTabList,
  TabLinkLabel,
} from "@/components/layout/scrollable-tab-list";
import { cn } from "@/lib/utils";

const TABS: {
  href: string;
  label: string;
  shortLabel?: string;
  match: (path: string) => boolean;
}[] = [
  { href: "/admin/members", label: "Members", match: (path: string) => path === "/admin/members" },
  { href: "/admin/sync", label: "Sync", match: (path: string) => path === "/admin/sync" },
  {
    href: "/admin/games",
    label: "Games",
    match: (path: string) => path === "/admin/games" || path.startsWith("/admin/games/"),
  },
  { href: "/admin/webhooks", label: "Webhooks", match: (path: string) => path === "/admin/webhooks" },
  {
    href: "/admin/game-of-the-week",
    label: "Game of the week",
    shortLabel: "GOTW",
    match: (path: string) => path === "/admin/game-of-the-week",
  },
];

export function AdminTabNav() {
  const pathname = usePathname();

  return (
    <ScrollableTabList ariaLabel="Admin sections" listClassName="-mb-px gap-6">
      {TABS.map((tab) => {
        const active = tab.match(pathname);
        return (
          <ScrollableTabItem key={tab.href}>
            <Link
              href={tab.href}
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
