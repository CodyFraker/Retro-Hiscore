"use client";

import Link from "next/link";
import type { AdminGameDto } from "@/generated/api-client";
import { ConsoleName } from "@/components/console-name";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { LeaderboardSyncTierBadge } from "@/components/sync/leaderboard-sync-tier-badge";

type Props = {
  games: AdminGameDto[];
};

export function AdminGamesSection({ games }: Props) {
  return (
    <section className="space-y-4">
      <ResponsiveTable
        rows={games}
        rowKey={(game) => game.id}
        columns={[
          {
            header: "Title",
            render: (game) => (
              <Link
                href={`/admin/games/${game.raGameId}`}
                className="font-medium hover:text-[var(--accent-retro)]"
              >
                {game.title}
              </Link>
            ),
          },
          {
            header: "Platform",
            render: (game) => (
              <ConsoleName
                name={game.consoleName}
                iconUrl={game.consoleIconUrl}
                fallback={`RA #${game.raGameId}`}
              />
            ),
          },
          {
            header: "Mirrors",
            headerClassName: "text-right",
            cellClassName: "text-right tabular-nums",
            render: (game) => game.sourceCount,
          },
          {
            header: "Metadata synced",
            cellClassName: "text-sm text-muted-foreground",
            render: (game) =>
              game.metadataSyncedAt ? (
                <FormattedSyncTime value={game.metadataSyncedAt} />
              ) : (
                "—"
              ),
          },
          {
            header: "LB sync",
            cellClassName: "text-sm",
            render: (game) => (
              <div className="space-y-1">
                <LeaderboardSyncTierBadge status={game.leaderboardSyncStatus} />
                <p className="text-xs text-muted-foreground">
                  {game.leaderboardSyncStatus.leaderboardSyncIsDue
                    ? "Due now"
                    : game.leaderboardSyncStatus.leaderboardSyncNextDueAt
                      ? (
                          <>
                            Next{" "}
                            <FormattedSyncTime
                              value={game.leaderboardSyncStatus.leaderboardSyncNextDueAt}
                            />
                          </>
                        )
                      : "—"}
                </p>
              </div>
            ),
          },
          {
            header: "Actions",
            headerClassName: "text-right",
            cellClassName: "text-right text-sm",
            render: (game) => (
              <Link
                href={`/games/${game.raGameId}`}
                className="text-muted-foreground hover:text-foreground"
              >
                View
              </Link>
            ),
          },
        ]}
        renderMobileCard={(game) => (
          <li key={game.id} className="rounded border border-border bg-card p-4 text-sm">
            <Link
              href={`/admin/games/${game.raGameId}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {game.title}
            </Link>
            <div className="mt-2">
              <ConsoleName
                name={game.consoleName}
                iconUrl={game.consoleIconUrl}
                fallback={`RA #${game.raGameId}`}
              />
            </div>
            <DataFieldList
              className="mt-3"
              fields={[
                { label: "Mirrors", value: game.sourceCount },
                {
                  label: "Metadata synced",
                  value: game.metadataSyncedAt ? (
                    <FormattedSyncTime value={game.metadataSyncedAt} />
                  ) : (
                    "—"
                  ),
                },
                {
                  label: "LB sync",
                  value: <LeaderboardSyncTierBadge status={game.leaderboardSyncStatus} />,
                },
              ]}
            />
            <Link
              href={`/games/${game.raGameId}`}
              className="mt-3 inline-block text-sm text-muted-foreground hover:text-foreground"
            >
              View public page
            </Link>
          </li>
        )}
      />
    </section>
  );
}
