"use client";

import { Menu, User } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { SyncHealthChip } from "@/components/layout/sync-health-chip";
import { HeaderUserMenu } from "@/components/header-user-menu";
import type { SyncHealthResponse } from "@/generated/api-client";
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
import {
  accountHeaderNavLinks,
  MAIN_HEADER_NAV_LINKS,
  type HeaderNavLink,
} from "@/lib/header-nav-links";
import { useTheme } from "@/components/theme/theme-provider";
import { cn } from "@/lib/utils";

type Props = {
  displayName: string;
  avatarUrl?: string | null;
  showAdminNav?: boolean;
  profileHref?: string | null;
  syncHealth?: SyncHealthResponse | null;
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
  links,
  ariaLabel = "Main",
}: {
  onNavigate?: () => void;
  links: HeaderNavLink[];
  ariaLabel?: string;
}) {
  const pathname = usePathname();

  return (
    <nav
      className={cn(
        "flex gap-1 md:gap-3 lg:gap-5",
        onNavigate ? "flex-col" : "flex-col md:flex-row md:flex-nowrap md:justify-center md:overflow-x-auto",
      )}
      aria-label={ariaLabel}
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
            {onNavigate || !link.shortLabel ? (
              link.label
            ) : (
              <>
                <span className="lg:hidden">{link.shortLabel}</span>
                <span className="hidden lg:inline">{link.label}</span>
              </>
            )}
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
  profileHref,
  syncHealth,
}: Props) {
  const [open, setOpen] = useState(false);
  const { theme } = useTheme();
  const isAero = theme === "frutiger-aero";

  return (
    <header
      data-site-header
      className={cn(
        "border-b border-border",
        isAero ? "glass-bg" : "bg-card",
      )}
    >
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 md:gap-6 md:py-4">
        <Link
          href="/"
          className="shrink-0 font-[family-name:var(--font-display)] text-xl tracking-wide text-[var(--accent-retro)] md:text-2xl"
        >
          Retro Hiscore
        </Link>

        <div className="hidden min-w-0 flex-1 md:flex md:justify-center">
          <NavLinks links={MAIN_HEADER_NAV_LINKS} />
        </div>

        <div className="hidden shrink-0 items-center gap-2 md:flex">
          {syncHealth ? <SyncHealthChip health={syncHealth} admin={showAdminNav} /> : null}
          <HeaderUserMenu
            displayName={displayName}
            avatarUrl={avatarUrl}
            profileHref={profileHref}
            showAdminNav={showAdminNav}
          />
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
              {syncHealth ? <SyncHealthChip health={syncHealth} admin={showAdminNav} /> : null}
              <NavLinks onNavigate={() => setOpen(false)} links={MAIN_HEADER_NAV_LINKS} />
              <div className="border-t border-border pt-4">
                <NavLinks
                  onNavigate={() => setOpen(false)}
                  links={accountHeaderNavLinks(showAdminNav)}
                  ariaLabel="Account"
                />
              </div>
              <Link
                href={profileHref ?? "/settings"}
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
                {profileHref ? (
                  <Button
                    variant="outline"
                    size="sm"
                    render={
                      <Link href={profileHref} onClick={() => setOpen(false)} />
                    }
                  >
                    <User />
                    Profile
                  </Button>
                ) : null}
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
