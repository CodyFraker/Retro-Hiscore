"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { ChartEmptyState } from "@/components/charts/chart-empty-state";
import { FriendRank } from "@/components/friend-rank";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { GameHistoryItemDto, GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";
import { formatFriendRankDelta } from "@/lib/game-delta-format";
import type { GameDelta } from "@/lib/game-history-series";
import {
  countDistinctSyncTimestampsForGameMemberBoards,
  gameMemberBoardRankSeriesHasVariation,
  indexGameDeltasByBoardMember,
  toGameMemberBoardRankSeries,
} from "@/lib/game-history-series";

const CHART_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

type Props = {
  items: GameHistoryItemDto[];
  leaderboards: GameLeaderboardDto[];
  members: StandingMemberDto[];
  deltas: GameDelta[];
  defaultMemberId?: string | null;
};

function formatTick(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function boardSeriesKey(raLeaderboardId: number) {
  return `board-${raLeaderboardId}`;
}

function resolveDefaultMemberId(
  members: StandingMemberDto[],
  defaultMemberId?: string | null,
): string | null {
  if (members.length === 0) {
    return null;
  }
  if (defaultMemberId && members.some((member) => member.id === defaultMemberId)) {
    return defaultMemberId;
  }
  return members[0]?.id ?? null;
}

export function GameRankTrendSection({
  items,
  leaderboards,
  members,
  deltas,
  defaultMemberId,
}: Props) {
  const deltaByKey = useMemo(() => indexGameDeltasByBoardMember(deltas), [deltas]);
  const initialMemberId = resolveDefaultMemberId(members, defaultMemberId);
  const [selectedMemberId, setSelectedMemberId] = useState<string | null>(initialMemberId);
  const [hiddenBoards, setHiddenBoards] = useState<Record<string, boolean>>({});

  const memberBoardSeries = useMemo(() => {
    if (!selectedMemberId) {
      return [];
    }
    return toGameMemberBoardRankSeries(selectedMemberId, items);
  }, [items, selectedMemberId]);

  const rankChartData = useMemo(() => {
    const byTime = new Map<string, Record<string, string | number>>();
    for (const board of memberBoardSeries) {
      const key = boardSeriesKey(board.raLeaderboardId);
      for (const point of board.points) {
        if (point.friendRank == null) {
          continue;
        }
        const row = byTime.get(point.syncedAt) ?? { syncedAt: point.syncedAt };
        row[key] = point.friendRank;
        byTime.set(point.syncedAt, row);
      }
    }
    return [...byTime.values()].sort(
      (a, b) =>
        new Date(String(a.syncedAt)).getTime() - new Date(String(b.syncedAt)).getTime(),
    );
  }, [memberBoardSeries]);

  const maxFriendRank = useMemo(() => {
    let max = 1;
    for (const board of memberBoardSeries) {
      for (const point of board.points) {
        if (point.friendRank != null && point.friendRank > max) {
          max = point.friendRank;
        }
      }
    }
    return max;
  }, [memberBoardSeries]);

  const hasRankTrend =
    countDistinctSyncTimestampsForGameMemberBoards(memberBoardSeries) >= 2 &&
    gameMemberBoardRankSeriesHasVariation(memberBoardSeries);

  if (members.length === 0 || leaderboards.length === 0) {
    return null;
  }

  return (
    <section className="space-y-6">
      <div className="space-y-3">
        <div>
          <h2 className="steam-section-heading">Friend ranks since last sync</h2>
          <p className="text-xs text-muted-foreground">
            Current friend rank on each board, with movement since the previous sync.
          </p>
        </div>
        <div className="md:overflow-x-auto">
          <ResponsiveTable
            rows={leaderboards}
            rowKey={(board) => String(board.id)}
            desktopClassName="overflow-x-auto rounded border border-border"
            renderDesktop={() => (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="min-w-48">Leaderboard</TableHead>
                    {members.map((member) => (
                      <TableHead key={member.id} className="min-w-28 text-right">
                        <Link
                          href={`/members/${encodeURIComponent(member.raUsername)}`}
                          className="inline-flex items-center justify-end gap-2 hover:text-[var(--accent-retro)]"
                        >
                          <MemberAvatar
                            avatarUrl={member.avatarUrl}
                            displayName={member.displayName}
                            size={20}
                          />
                          {member.displayName}
                        </Link>
                      </TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {leaderboards.map((board) => (
                    <TableRow key={board.id}>
                      <TableCell>
                        <Link
                          href={`/leaderboards/${board.raLeaderboardId}`}
                          className="font-medium hover:text-[var(--accent-retro)]"
                        >
                          {board.title}
                        </Link>
                      </TableCell>
                      {members.map((member) => {
                        const standing = board.standings.find((s) => s.memberId === member.id);
                        const delta = deltaByKey.get(`${board.raLeaderboardId}:${member.id}`);
                        return (
                          <TableCell key={member.id} className="text-right text-sm">
                            <FriendRank
                              rank={standing?.friendRank}
                              className="justify-end font-mono"
                              iconClassName="size-3"
                            />
                            {delta?.friendRankDelta != null && delta.friendRankDelta !== 0 && (
                              <p className="mt-0.5 font-mono text-xs text-[var(--accent-retro)]">
                                {formatFriendRankDelta(delta.friendRankDelta)}
                              </p>
                            )}
                          </TableCell>
                        );
                      })}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
            renderMobileCard={(board) => (
              <li key={board.id} className="rounded border border-border bg-card p-4">
                <Link
                  href={`/leaderboards/${board.raLeaderboardId}`}
                  className="font-medium hover:text-[var(--accent-retro)]"
                >
                  {board.title}
                </Link>
                <ul className="mt-3 divide-y divide-border border-t border-border">
                  {members.map((member) => {
                    const standing = board.standings.find((s) => s.memberId === member.id);
                    const delta = deltaByKey.get(`${board.raLeaderboardId}:${member.id}`);
                    return (
                      <li
                        key={member.id}
                        className="flex items-center justify-between gap-3 py-2.5 text-sm"
                      >
                        <span className="truncate text-muted-foreground">{member.displayName}</span>
                        <div className="shrink-0 text-right">
                          <FriendRank rank={standing?.friendRank} className="justify-end font-mono" />
                          {delta?.friendRankDelta != null && delta.friendRankDelta !== 0 && (
                            <p className="mt-0.5 font-mono text-xs text-[var(--accent-retro)]">
                              {formatFriendRankDelta(delta.friendRankDelta)}
                            </p>
                          )}
                        </div>
                      </li>
                    );
                  })}
                </ul>
              </li>
            )}
          />
        </div>
      </div>

      <div className="space-y-3">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 className="steam-section-heading">Friend rank over time</h2>
            <p className="text-xs text-muted-foreground">
              One line per board for the selected member (same view as member Trends).
            </p>
          </div>
          <label className="flex flex-col gap-1">
            <span className="text-xs font-medium text-muted-foreground">Member</span>
            <select
              value={selectedMemberId ?? ""}
              onChange={(event) => setSelectedMemberId(event.target.value || null)}
              className="h-8 min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:min-w-48"
            >
              {members.map((member) => (
                <option key={member.id} value={member.id}>
                  {member.displayName}
                </option>
              ))}
            </select>
          </label>
        </div>
        {hasRankTrend ? (
          <div className="min-w-0 h-56 w-full rounded border border-border bg-secondary/20 p-3 sm:h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={rankChartData} margin={{ top: 8, right: 4, left: 0, bottom: 0 }}>
                <CartesianGrid stroke="var(--border)" strokeDasharray="3 3" />
                <XAxis
                  dataKey="syncedAt"
                  tickFormatter={formatTick}
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                  minTickGap={32}
                />
                <YAxis
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                  width={40}
                  reversed
                  domain={[1, maxFriendRank]}
                  allowDecimals={false}
                />
                <Tooltip
                  contentStyle={{
                    background: "var(--card)",
                    border: "1px solid var(--border)",
                    borderRadius: "0.4rem",
                    color: "var(--foreground)",
                  }}
                  labelFormatter={(label) => formatTick(String(label))}
                  formatter={(value, name) => {
                    const board = memberBoardSeries.find(
                      (item) => boardSeriesKey(item.raLeaderboardId) === String(name),
                    );
                    const label = board?.leaderboardTitle ?? String(name);
                    return [`#${value}`, label];
                  }}
                />
                <Legend
                  wrapperStyle={{ flexWrap: "wrap", paddingTop: 8, maxHeight: 96, overflowY: "auto" }}
                  onClick={(payload) => {
                    const id = String(payload.dataKey ?? "");
                    if (!id) return;
                    setHiddenBoards((prev) => ({ ...prev, [id]: !prev[id] }));
                  }}
                />
                {memberBoardSeries.map((board, index) => {
                  const key = boardSeriesKey(board.raLeaderboardId);
                  return (
                    <Line
                      key={key}
                      type="monotone"
                      dataKey={key}
                      name={board.leaderboardTitle}
                      stroke={CHART_COLORS[index % CHART_COLORS.length]}
                      strokeWidth={2}
                      dot={{ r: 3 }}
                      connectNulls
                      hide={Boolean(hiddenBoards[key])}
                    />
                  );
                })}
              </LineChart>
            </ResponsiveContainer>
          </div>
        ) : (
          <ChartEmptyState message="No rank movement for this member yet. Charts unlock after the next sync with changes." />
        )}
      </div>
    </section>
  );
}
