"use client";

import { Menu } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { HeaderUserMenu } from "@/components/header-user-menu";
import { SettingsLinkButton } from "@/components/settings-link-button";
import { SignOutButton } from "@/components/sign-out-button";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { cn } from "@/lib/utils";

type Props = {
  displayName: string;
  avatarUrl?: string | null;
  showAdminNav?: boolean;
};

type NavLink = {
  href: string;
  label: string;
  match: (pathname: string) => boolean;
};

const BASE_NAV_LINKS: NavLink[] = [
  { href: "/", label: "Dashboard", match: (path) => path === "/" },
  { href: "/games", label: "Games", match: (path) => path === "/games" || path.startsWith("/games/") },
  { href: "/achievements", label: "Achievements", match: (path) => path === "/achievements" || path.startsWith("/achievements/") },
  { href: "/members", label: "Members", match: (path) => path === "/members" || path.startsWith("/members/") },
  { href: "/settings", label: "Settings", match: (path) => path === "/settings" || path.startsWith("/settings/") },
];

const ADMIN_NAV_LINK: NavLink = {
  href: "/admin/members",
  label: "Admin",
  match: (path) => path === "/admin" || path.startsWith("/admin/"),
};

function navLinkClassName(active: boolean, mobile: boolean) {
  return cn(
    "text-sm font-medium transition-colors",
    mobile
      ? cn(
          "rounded-lg px-3 py-2.5",
          active
            ? "bg-secondary/60 text-foreground"
            : "text-muted-foreground hover:bg-secondary/60 hover:text-foreground",
        )
      : cn(
          "inline-block border-b-2 pb-1 whitespace-nowrap",
          active
            ? "border-[var(--accent-retro)] text-foreground"
            : "border-transparent text-muted-foreground hover:border-border hover:text-foreground",
        ),
  );
}

function NavLinks({
  onNavigate,
  showAdminNav,
}: {
  onNavigate?: () => void;
  showAdminNav?: boolean;
}) {
  const pathname = usePathname();
  const links = showAdminNav ? [...BASE_NAV_LINKS, ADMIN_NAV_LINK] : BASE_NAV_LINKS;

  return (
    <nav
      className={cn(
        "flex gap-1 md:gap-5",
        onNavigate ? "flex-col" : "flex-col md:flex-row md:flex-nowrap md:justify-center md:overflow-x-auto",
      )}
      aria-label="Main"
    >
      {links.map((link) => {
        const active = link.match(pathname);
        return (
          <Link
            key={link.href}
            href={link.href}
            onClick={onNavigate}
            className={navLinkClassName(active, Boolean(onNavigate))}
            aria-current={active ? "page" : undefined}
          >
            {link.label}
          </Link>
        );
      })}
    </nav>
  );
}

export function SiteHeaderBar({
  displayName,
  avatarUrl,
  showAdminNav,
}: Props) {
  const [open, setOpen] = useState(false);

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 md:gap-6 md:py-4">
        <Link
          href="/"
          className="shrink-0 font-[family-name:var(--font-display)] text-xl tracking-wide text-[var(--accent-retro)] md:text-2xl"
        >
          Retro Hiscore
        </Link>

        <div className="hidden min-w-0 flex-1 md:flex md:justify-center">
          <NavLinks showAdminNav={showAdminNav} />
        </div>

        <div className="hidden shrink-0 md:flex">
          <HeaderUserMenu displayName={displayName} avatarUrl={avatarUrl} />
        </div>

        <Sheet open={open} onOpenChange={setOpen}>
          <SheetTrigger
            render={
              <Button
                variant="outline"
                size="icon"
                className="min-h-11 min-w-11 md:hidden"
                aria-label="Open menu"
              />
            }
          >
            <Menu />
          </SheetTrigger>
          <SheetContent side="right" className="w-[min(100vw-2rem,20rem)]">
            <SheetHeader>
              <SheetTitle>Menu</SheetTitle>
            </SheetHeader>
            <div className="flex flex-col gap-6 overflow-y-auto px-4 pb-6">
              <NavLinks onNavigate={() => setOpen(false)} showAdminNav={showAdminNav} />
              <Link
                href="/settings"
                onClick={() => setOpen(false)}
                className="flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-foreground"
              >
                {avatarUrl && (
                  <Image
                    src={avatarUrl}
                    alt=""
                    width={28}
                    height={28}
                    className="rounded-full"
                  />
                )}
                <span>{displayName}</span>
              </Link>
              <div className="flex flex-col gap-3 border-t border-border pt-4">
                <SettingsLinkButton onNavigate={() => setOpen(false)} />
                <SignOutButton />
              </div>
            </div>
          </SheetContent>
        </Sheet>
      </div>
    </header>
  );
}
