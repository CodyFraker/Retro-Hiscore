"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

const TABS = [
  { href: "/admin/members", label: "Members", match: (path: string) => path === "/admin/members" },
  { href: "/admin/sync", label: "Sync", match: (path: string) => path === "/admin/sync" },
  {
    href: "/admin/games",
    label: "Games",
    match: (path: string) => path === "/admin/games" || path.startsWith("/admin/games/"),
  },
] as const;

export function AdminTabNav() {
  const pathname = usePathname();

  return (
    <nav className="border-b border-border" aria-label="Admin sections">
      <ul className="-mb-px flex gap-6 overflow-x-auto">
        {TABS.map((tab) => {
          const active = tab.match(pathname);
          return (
            <li key={tab.href}>
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
                {tab.label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
