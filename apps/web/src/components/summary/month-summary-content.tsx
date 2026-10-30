import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { DataFieldList } from "@/components/layout/data-field-list";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { MonthSummaryResponse } from "@/generated/api-client";

type Props = {
  summary: MonthSummaryResponse;
};

function monthTitle(year: number, month: number) {
  return new Date(Date.UTC(year, month - 1, 1)).toLocaleString(undefined, {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  });
}

export function MonthSummaryContent({ summary }: Props) {
  const { groupTotals } = summary;
  const hasActivity =
    groupTotals.gamesPlayed > 0 ||
    groupTotals.achievementsUnlocked > 0 ||
    groupTotals.boardsTaken > 0 ||
    groupTotals.scoresImproved > 0;

  return (
    <div className="space-y-8">
      <Card>
        <CardHeader>
          <CardTitle>Group totals · {monthTitle(summary.year, summary.month)}</CardTitle>
        </CardHeader>
        <CardContent>
          {!hasActivity ? (
            <p className="text-sm text-muted-foreground">No group activity recorded this month yet.</p>
          ) : (
            <DataFieldList
              fields={[
                { label: "Games played", value: groupTotals.gamesPlayed.toLocaleString() },
                { label: "Achievements", value: groupTotals.achievementsUnlocked.toLocaleString() },
                { label: "Hardcore unlocks", value: groupTotals.hardcoreUnlocks.toLocaleString() },
                { label: "Hardcore points", value: groupTotals.hardcorePoints.toLocaleString() },
                { label: "True points", value: groupTotals.truePoints.toLocaleString() },
                { label: "Boards taken", value: groupTotals.boardsTaken.toLocaleString() },
                { label: "Score improvements", value: groupTotals.scoresImproved.toLocaleString() },
              ]}
            />
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Members</CardTitle>
        </CardHeader>
        <CardContent className="px-0">
          <ul className="divide-y divide-border">
            {summary.memberRows.map((row) => (
              <li
                key={row.memberId}
                className="space-y-2 px-(--card-spacing) py-3 text-sm"
              >
                <Link
                  href={`/members/${encodeURIComponent(row.raUsername)}`}
                  className="flex min-w-0 items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
                >
                  <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={28} />
                  <span className="truncate">{row.displayName}</span>
                </Link>
                <DataFieldList
                  fields={[
                    { label: "Games", value: row.gamesPlayed },
                    { label: "Achievements", value: row.achievementsUnlocked },
                    { label: "Hardcore pts", value: row.hardcorePoints },
                    { label: "Boards taken", value: row.boardsTaken },
                    { label: "Score improvements", value: row.scoresImproved },
                  ]}
                />
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Most active games</CardTitle>
        </CardHeader>
        <CardContent className="px-0">
          {summary.mostActiveGames.length === 0 ? (
            <p className="px-(--card-spacing) text-sm text-muted-foreground">
              No game activity this month yet.
            </p>
          ) : (
            <ul className="divide-y divide-border">
              {summary.mostActiveGames.map((game) => (
                <li
                  key={game.raGameId}
                  className="flex flex-col gap-1 px-(--card-spacing) py-3 text-sm sm:flex-row sm:items-center sm:justify-between"
                >
                  {game.isTracked ? (
                    <Link
                      href={`/games/${game.raGameId}`}
                      className="font-medium hover:text-[var(--accent-retro)]"
                    >
                      {game.title}
                    </Link>
                  ) : (
                    <span className="font-medium">{game.title}</span>
                  )}
                  <DataFieldList
                    className="sm:justify-end"
                    fields={[
                      { label: "Unlocks", value: game.achievementsUnlocked },
                      { label: "Score improvements", value: game.scoreImprovements },
                      { label: "Members", value: game.memberCount },
                    ]}
                  />
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
