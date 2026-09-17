import { ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberAvatar } from "@/components/members/member-avatar";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { Badge } from "@/components/ui/badge";
import type { DashboardAchievementActivityItemDto } from "@/generated/api-client";

type Props = {
  items: DashboardAchievementActivityItemDto[];
  variant: "card" | "page" | "embedded";
  emptyMessage?: string;
  showMember?: boolean;
  showGameLink?: boolean;
};

const defaultEmpty =
  "Shows up after achievement sync on tracked games. Run Admin → Sync achievements or member rank sync.";

export function AchievementsActivityFeed({
  items,
  variant,
  emptyMessage = defaultEmpty,
  showMember = true,
  showGameLink = true,
}: Props) {
  const badgeSize =
    variant === "page" ? "h-12 w-12" : variant === "embedded" ? "h-10 w-10" : "h-10 w-10";
  const badgeSizesAttr = variant === "page" ? "48px" : "40px";
  const rowPadding =
    variant === "page" ? "py-4" : variant === "embedded" ? "py-2 first:pt-0 last:pb-0" : "py-3 first:pt-0 last:pb-0";
  const titleClass =
    variant === "page" ? "text-base" : variant === "embedded" ? "text-sm" : "text-sm";

  if (items.length === 0) {
    return <p className="text-sm text-muted-foreground">{emptyMessage}</p>;
  }

  return (
    <ul className="divide-y divide-border">
      {items.map((item) => (
        <li
          key={`${item.memberId}-${item.raAchievementId}-${item.dateEarned}`}
          className={`flex gap-3 ${rowPadding}`}
        >
          <AchievementBadgeThumb
            badgeUrl={item.badgeUrl}
            title={item.title}
            sizeClass={badgeSize}
            sizesAttr={badgeSizesAttr}
          />
          <div className="min-w-0 flex-1 space-y-0.5">
            <p className={`flex items-start gap-2 leading-snug ${titleClass}`}>
              {showMember && item.raUsername ? (
                <Link
                  href={`/members/${encodeURIComponent(item.raUsername)}`}
                  className="mt-0.5 shrink-0 rounded-full ring-offset-background transition-opacity hover:opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  title={item.displayName}
                >
                  <MemberAvatar
                    avatarUrl={item.avatarUrl}
                    displayName={item.displayName}
                    size={32}
                  />
                </Link>
              ) : null}
              <span className="min-w-0 pt-0.5">
                <span className="text-muted-foreground">Unlocked </span>
                <MemberRaGameLink
                  raGameId={item.raGameId}
                  isTracked
                  className="font-medium hover:text-[var(--accent-retro)]"
                >
                  {item.title}
                </MemberRaGameLink>
                <AchievementPointsMeta item={item} variant={variant} />
              </span>
            </p>
            {showGameLink ? (
              <MemberRaGameLink
                raGameId={item.raGameId}
                isTracked
                className="inline-block text-xs font-medium text-muted-foreground hover:text-[var(--accent-retro)] hover:underline"
              >
                {item.gameTitle}
              </MemberRaGameLink>
            ) : null}
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

function AchievementBadgeThumb({
  badgeUrl,
  title,
  sizeClass,
  sizesAttr,
}: {
  badgeUrl?: string | null;
  title: string;
  sizeClass: string;
  sizesAttr: string;
}) {
  return (
    <div
      className={`relative ${sizeClass} shrink-0 overflow-hidden rounded-md bg-secondary/40 steam-bevel-inset`}
    >
      {badgeUrl ? (
        <Image
          src={badgeUrl}
          alt={title}
          fill
          className="object-contain p-0.5"
          sizes={sizesAttr}
        />
      ) : (
        <div className="flex h-full items-center justify-center text-muted-foreground">
          <ImageOff className="size-4" aria-hidden />
        </div>
      )}
    </div>
  );
}

function AchievementPointsMeta({
  item,
  variant,
}: {
  item: DashboardAchievementActivityItemDto;
  variant: Props["variant"];
}) {
  const pointsClass =
    variant === "page" ? "font-mono text-sm text-muted-foreground" : "text-xs text-muted-foreground";

  return (
    <>
      <span className={pointsClass}> +{item.points}</span>
      {item.hardcoreEarned ? (
        <Badge variant="outline" className="ml-1.5 align-middle text-[10px] uppercase">
          HC
        </Badge>
      ) : null}
    </>
  );
}
