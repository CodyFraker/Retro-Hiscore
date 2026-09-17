import { Award, ImageOff, Library, TableProperties } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberAvatar } from "@/components/members/member-avatar";
import type { DashboardGroupActivityItemDto } from "@/generated/api-client";

type Props = {
  items: DashboardGroupActivityItemDto[];
  emptyMessage?: string;
};

export function GroupActivityFeed({ items, emptyMessage }: Props) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        {emptyMessage ??
          "Nothing in the last leaderboard sync window yet. Run a sync or check back after the next scheduled refresh."}
      </p>
    );
  }

  return (
    <ul className="divide-y divide-border">
      {items.map((item, index) => (
        <li key={`${item.kind}-${item.occurredAt}-${index}`} className="flex gap-3 py-3">
          <ActivityThumbnail item={item} />
          <div className="min-w-0 flex-1 space-y-0.5">
            <p className="flex items-start gap-2 text-sm leading-snug">
              {item.memberRaUsername && item.memberDisplayName ? (
                <Link
                  href={`/members/${encodeURIComponent(item.memberRaUsername)}`}
                  className="mt-0.5 shrink-0 rounded-full ring-offset-background transition-opacity hover:opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  title={item.memberDisplayName}
                >
                  <MemberAvatar
                    avatarUrl={item.memberAvatarUrl}
                    displayName={item.memberDisplayName}
                    size={32}
                  />
                </Link>
              ) : null}
              <span className="min-w-0 pt-0.5">
                <ActivityBody item={item} />
              </span>
            </p>
            {item.subtitle ? (
              <p className="text-xs text-muted-foreground">{item.subtitle}</p>
            ) : null}
            <p className="text-xs text-muted-foreground">
              <FormattedSyncTime value={item.occurredAt} />
            </p>
          </div>
        </li>
      ))}
    </ul>
  );
}

function ActivityThumbnail({ item }: { item: DashboardGroupActivityItemDto }) {
  const gameArtUrl = item.imageBoxArtUrl ?? item.imageIconUrl;
  const artUrl =
    gameArtUrl ?? (item.kind === "AchievementUnlock" ? item.badgeUrl : null);

  const KindIcon =
    item.kind === "AchievementUnlock"
      ? Award
      : item.kind === "GameTracked"
        ? Library
        : TableProperties;

  const kindLabel =
    item.kind === "AchievementUnlock"
      ? "Achievement unlock"
      : item.kind === "GameTracked"
        ? "New tracked game"
        : "Leaderboard score";

  return (
    <div className="relative h-12 w-12 shrink-0">
      <div className="relative h-full w-full overflow-hidden rounded-md bg-secondary/40 steam-bevel-inset">
        {artUrl ? (
          <Image
            src={artUrl}
            alt=""
            fill
            className={item.kind === "AchievementUnlock" ? "object-contain p-0.5" : "object-cover"}
            sizes="48px"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-muted-foreground">
            <ImageOff className="size-4" />
          </div>
        )}
      </div>
      <span
        className="absolute -bottom-1 -right-1 flex size-5 items-center justify-center rounded-full border border-border bg-background text-muted-foreground shadow-sm"
        title={kindLabel}
      >
        <KindIcon className="size-3" aria-hidden />
        <span className="sr-only">{kindLabel}</span>
      </span>
    </div>
  );
}

function ActivityBody({ item }: { item: DashboardGroupActivityItemDto }) {
  if (item.kind === "GameTracked" && item.raGameId != null) {
    const label = item.title.replace(/ added to tracked games$/, "");
    return (
      <Link href={`/games/${item.raGameId}`} className="hover:text-[var(--accent-retro)]">
        <span className="font-medium">{label}</span>
        <span className="text-muted-foreground"> added to tracked games</span>
      </Link>
    );
  }

  if (item.kind === "AchievementUnlock" && item.achievementTitle) {
    return (
      <>
        <span className="text-muted-foreground">Unlocked </span>
        <span className="font-medium">{item.achievementTitle}</span>
      </>
    );
  }

  if (item.kind === "LeaderboardMove" && item.leaderboardTitle) {
    const verb =
      item.friendRankDelta != null && item.friendRankDelta !== 0 ? "Moved on" : "Scored on";
    return (
      <>
        <span className="text-muted-foreground">{verb} </span>
        <span className="font-medium">{item.leaderboardTitle}</span>
        {item.raLeaderboardId != null ? (
          <>
            <span className="text-muted-foreground"> · </span>
            <Link
              href={`/leaderboards/${item.raLeaderboardId}`}
              className="text-[var(--accent-retro)] hover:underline"
            >
              View board
            </Link>
          </>
        ) : null}
      </>
    );
  }

  return <ActivityTitleFallback item={item} />;
}

function ActivityTitleFallback({ item }: { item: DashboardGroupActivityItemDto }) {
  if (item.raGameId != null && item.kind === "GameTracked") {
    return (
      <Link href={`/games/${item.raGameId}`} className="hover:text-[var(--accent-retro)]">
        {item.title}
      </Link>
    );
  }

  if (item.raGameId != null && item.raLeaderboardId != null) {
    return (
      <>
        <span>{item.title}</span>
        <span className="text-muted-foreground"> · </span>
        <Link
          href={`/leaderboards/${item.raLeaderboardId}`}
          className="text-[var(--accent-retro)] hover:underline"
        >
          View board
        </Link>
      </>
    );
  }

  if (item.raGameId != null) {
    return (
      <Link href={`/games/${item.raGameId}`} className="hover:text-[var(--accent-retro)]">
        {item.title}
      </Link>
    );
  }

  return <span>{item.title}</span>;
}
