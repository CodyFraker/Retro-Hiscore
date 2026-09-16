import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
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
        <li key={`${item.kind}-${item.occurredAt}-${index}`} className="py-3 text-sm">
          <p className="leading-snug">
            <ActivityTitle item={item} />
          </p>
          {item.subtitle ? (
            <p className="mt-0.5 text-xs text-muted-foreground">{item.subtitle}</p>
          ) : null}
          <p className="mt-1 text-xs text-muted-foreground">
            <FormattedSyncTime value={item.occurredAt} />
          </p>
        </li>
      ))}
    </ul>
  );
}

function ActivityTitle({ item }: { item: DashboardGroupActivityItemDto }) {
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
          className="hover:text-[var(--accent-retro)]"
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
