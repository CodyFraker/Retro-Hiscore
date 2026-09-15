"use client";

import { ExternalLink } from "lucide-react";
import type { MouseEvent } from "react";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { retroAchievementsUserUrl } from "@/lib/ra-member-metrics";

type Props = {
  raUsername?: string | null;
  raUlid?: string | null;
  className?: string;
  iconOnly?: boolean;
};

export function RetroachievementsUserLink({
  raUsername,
  raUlid,
  className,
  iconOnly = false,
}: Props) {
  const href = retroAchievementsUserUrl(raUlid, raUsername);
  if (!href) {
    return null;
  }

  function handleClick(event: MouseEvent<HTMLAnchorElement>) {
    event.stopPropagation();
  }

  if (iconOnly) {
    return (
      <a
        href={href}
        target="_blank"
        rel="noopener noreferrer"
        aria-label="View on RetroAchievements"
        onClick={handleClick}
        className={cn(
          buttonVariants({ variant: "outline", size: "icon-sm" }),
          className,
        )}
      >
        <ExternalLink className="size-4" />
      </a>
    );
  }

  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      onClick={handleClick}
      className={cn(buttonVariants({ variant: "outline" }), "w-full sm:w-auto", className)}
    >
      View on RetroAchievements
      <ExternalLink className="size-4" />
    </a>
  );
}
