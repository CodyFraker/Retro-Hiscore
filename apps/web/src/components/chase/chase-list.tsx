import Link from "next/link";
import type { MemberChaseItemDto } from "@/generated/api-client";

type Props = {
  items: MemberChaseItemDto[];
  compact?: boolean;
};

function formatGap(item: MemberChaseItemDto) {
  return item.scoreGap.toLocaleString();
}

export function ChaseList({ items, compact }: Props) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        You hold friend rank 2 on no boards right now. When you are close to the lead, those boards show up here.
      </p>
    );
  }

  const visible = compact ? items.slice(0, 5) : items;

  return (
    <ul className="divide-y divide-border">
      {visible.map((item) => (
        <li key={`${item.raLeaderboardId}`} className="flex flex-col gap-2 py-3 text-sm first:pt-0 last:pb-0">
          <div className="flex flex-wrap items-baseline justify-between gap-2">
            <Link
              href={`/games/${item.raGameId}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {item.gameTitle}
            </Link>
            <Link
              href={`/leaderboards/${item.raLeaderboardId}`}
              className="text-xs text-muted-foreground hover:text-[var(--accent-retro)]"
            >
              {item.leaderboardTitle}
            </Link>
          </div>
          <p className="text-muted-foreground">
            <Link
              href={`/members/${encodeURIComponent(item.leaderRaUsername)}`}
              className="text-foreground hover:text-[var(--accent-retro)]"
            >
              {item.leaderDisplayName}
            </Link>{" "}
            leads{" "}
            <span className="font-mono text-foreground">{item.leaderFormattedScore}</span>
            {" · "}
            you{" "}
            <span className="font-mono text-foreground">{item.myFormattedScore}</span>
            {" · "}
            gap{" "}
            <span className="font-mono text-[var(--accent-retro)]">{formatGap(item)}</span>
          </p>
        </li>
      ))}
    </ul>
  );
}
