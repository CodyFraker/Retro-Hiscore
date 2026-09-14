"use client";

import { Menu } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import { RefreshButton } from "@/components/refresh-button";
import { RefreshMetadataButton } from "@/components/refresh-metadata-button";
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

type Props = {
  displayName: string;
  avatarUrl?: string | null;
  syncLabel: string;
  metadataLabel: string;
  showAdminNav?: boolean;
};

const BASE_NAV_LINKS = [
  { href: "/", label: "Games" },
  { href: "/members", label: "Members" },
  { href: "/members#head-to-head", label: "Rivalry" },
  { href: "/settings", label: "Settings" },
] as const;

function NavLinks({
  onNavigate,
  showAdminNav,
}: {
  onNavigate?: () => void;
  showAdminNav?: boolean;
}) {
  const links = showAdminNav
    ? [
        ...BASE_NAV_LINKS,
        { href: "/admin/games", label: "Manage Games" },
        { href: "/admin", label: "Admin" },
      ]
    : BASE_NAV_LINKS;

  return (
    <nav className="flex flex-col gap-1 md:mt-2 md:flex-row md:gap-4">
      {links.map((link) => (
        <Link
          key={link.href}
          href={link.href}
          onClick={onNavigate}
          className="rounded-lg px-3 py-2.5 text-sm text-muted-foreground hover:bg-secondary/60 hover:text-foreground md:px-0 md:py-0 md:hover:bg-transparent"
        >
          {link.label}
        </Link>
      ))}
    </nav>
  );
}

export function SiteHeaderBar({
  displayName,
  avatarUrl,
  syncLabel,
  metadataLabel,
  showAdminNav,
}: Props) {
  const [open, setOpen] = useState(false);

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 md:py-5">
        <div className="min-w-0 flex-1 md:flex-none">
          <Link
            href="/"
            className="font-[family-name:var(--font-display)] text-xl tracking-wide text-[var(--accent-retro)] md:text-2xl"
          >
            Retro Hiscore
          </Link>
          <div className="hidden md:block">
            <NavLinks showAdminNav={showAdminNav} />
            <p className="mt-2 truncate text-sm text-muted-foreground" title={syncLabel}>
              {syncLabel}
            </p>
            {metadataLabel && (
              <p className="truncate text-xs text-muted-foreground" title={metadataLabel}>
                {metadataLabel}
              </p>
            )}
          </div>
        </div>

        <div className="hidden items-start gap-3 md:flex">
          <Link
            href="/settings"
            className="flex items-center gap-2 rounded-lg text-sm text-muted-foreground transition-colors hover:text-foreground"
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
          <RefreshMetadataButton />
          <RefreshButton />
          <SettingsLinkButton />
          <SignOutButton />
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
              <div className="space-y-1 border-t border-border pt-4 text-sm text-muted-foreground">
                <p>{syncLabel}</p>
                {metadataLabel && <p className="text-xs">{metadataLabel}</p>}
              </div>
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
                <RefreshMetadataButton />
                <RefreshButton />
                <SignOutButton />
              </div>
            </div>
          </SheetContent>
        </Sheet>
      </div>
    </header>
  );
}
