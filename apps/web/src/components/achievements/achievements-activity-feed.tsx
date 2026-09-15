import { ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { Badge } from "@/components/ui/badge";
import type { DashboardAchievementActivityItemDto } from "@/generated/api-client";

type Props = {
  items: DashboardAchievementActivityItemDto[];
  variant: "card" | "page";
  emptyMessage?: string;
};

const defaultEmpty =
  "Shows up after achievement sync on tracked games. Run Admin → Sync achievements or member rank sync.";

export function AchievementsActivityFeed({ items, variant, emptyMessage = defaultEmpty }: Props) {
  const badgeSize = variant === "page" ? "h-12 w-12" : "h-10 w-10";
  const badgeSizesAttr = variant === "page" ? "48px" : "40px";

  if (items.length === 0) {
    return <p className="text-sm text-muted-foreground">{emptyMessage}</p>;
  }

  return (
    <ul className="divide-y divide-border">
      {items.map((item) => (
        <li
          key={`${item.memberId}-${item.raAchievementId}-${item.dateEarned}`}
          className={`flex gap-3 ${variant === "page" ? "py-4" : "py-3 first:pt-0 last:pb-0"}`}
        >
          <div className={`relative ${badgeSize} shrink-0 overflow-hidden rounded bg-secondary/40`}>
            {item.badgeUrl ? (
              <Image
                src={item.badgeUrl}
                alt=""
                fill
                className="object-contain p-0.5"
                sizes={badgeSizesAttr}
              />
            ) : (
              <div className="flex h-full items-center justify-center text-muted-foreground">
                <ImageOff className="size-4" />
              </div>
            )}
          </div>
          <div className="min-w-0 flex-1 space-y-0.5">
            <p className={`leading-snug ${variant === "page" ? "text-base" : "text-sm"}`}>
              <Link
                href={`/members/${encodeURIComponent(item.raUsername)}`}
                className="font-medium hover:text-[var(--accent-retro)]"
              >
                {item.displayName}
              </Link>
              <span className="text-muted-foreground"> · </span>
              <span>{item.title}</span>
              {variant === "page" ? (
                <>
                  <span className="font-mono text-sm text-muted-foreground"> +{item.points}</span>
                  {item.hardcoreEarned ? (
                    <Badge variant="outline" className="ml-2 text-[10px] uppercase">
                      HC
                    </Badge>
                  ) : null}
                </>
              ) : null}
            </p>
            <MemberRaGameLink
              raGameId={item.raGameId}
              isTracked
              className="text-xs text-muted-foreground hover:text-foreground hover:underline"
            >
              {item.gameTitle}
            </MemberRaGameLink>
            {item.dateEarned ? (
              <p className="text-xs text-muted-foreground">
                <FormattedSyncTime value={item.dateEarned} />
              </p>
            ) : null}
          </div>
        </li>
      ))}
    </ul>
  );
}
