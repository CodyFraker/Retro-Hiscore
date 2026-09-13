"use client";

import Link from "next/link";
import { useState } from "react";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import type { ActivityItemDto } from "@/generated/api-client";

type Props = {
  items: ActivityItemDto[];
};

const DEFAULT_LIMIT = 8;

function formatSigned(value: number) {
  const abs = Math.abs(value).toLocaleString();
  if (value > 0) return `+${abs}`;
  if (value < 0) return `−${abs}`;
  return "0";
}

function ActivityItemRow({ item }: { item: ActivityItemDto }) {
  return (
    <li className="flex flex-col gap-2 py-3 text-sm">
      <div>
        <span className="font-medium">{item.displayName}</span>
        <span className="text-muted-foreground">
          {" "}
          ·{" "}
          <Link
            href={`/games/${item.raGameId}`}
            className="hover:text-[var(--accent-retro)]"
          >
            {item.gameTitle}
          </Link>
          {" "}
          ·{" "}
          <Link
            href={`/leaderboards/${item.raLeaderboardId}`}
            className="hover:text-[var(--accent-retro)]"
          >
            {item.leaderboardTitle}
          </Link>
        </span>
      </div>
      <div className="flex flex-wrap gap-4 font-mono text-xs text-muted-foreground">
        {item.scoreDelta != null && (
          <span>
            Score <span className="text-foreground">{formatSigned(item.scoreDelta)}</span>
          </span>
        )}
        {item.friendRankDelta != null && (
          <span>
            Rank{" "}
            <span className="text-foreground">
              {item.friendRankDelta > 0
                ? `↑${item.friendRankDelta}`
                : item.friendRankDelta < 0
                  ? `↓${Math.abs(item.friendRankDelta)}`
                  : "—"}
            </span>
          </span>
        )}
      </div>
    </li>
  );
}

export function ActivityFeed({ items }: Props) {
  const [expanded, setExpanded] = useState(false);

  if (items.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Since last sync</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Activity appears after your second score sync.
          </p>
        </CardContent>
      </Card>
    );
  }

  const visibleItems = expanded ? items : items.slice(0, DEFAULT_LIMIT);
  const hasMore = items.length > DEFAULT_LIMIT;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Since last sync</CardTitle>
      </CardHeader>
      <CardContent className="px-0">
        <ul className="divide-y divide-border px-(--card-spacing)">
          {visibleItems.map((item) => (
            <ActivityItemRow key={`${item.raLeaderboardId}-${item.memberId}`} item={item} />
          ))}
        </ul>
      </CardContent>
      {hasMore && (
        <CardFooter className="border-t">
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => setExpanded((value) => !value)}
          >
            {expanded ? "Show less" : `Show all ${items.length} changes`}
          </Button>
        </CardFooter>
      )}
    </Card>
  );
}
