"use client";

import { useState, useTransition } from "react";
import { Crown, Podium } from "lucide-react";
import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import type {
  ChampionshipRowDto,
  SeasonMonthDto,
  SeasonStandingsResponse,
  SeasonStandingsRowDto,
} from "@/generated/api-client";
import { loadSeasonStandingsAction } from "@/lib/actions/season-standings";
import { rivalryPath } from "@/lib/rivalry-path";
import { cn } from "@/lib/utils";

type Props = {
  championship: ChampionshipRowDto[];
  memberCount: number;
  initialSeason: SeasonStandingsResponse;
  limit?: number;
};

function championshipLeadMargin(rows: ChampionshipRowDto[]): number | null {
  if (rows.length < 2 || rows[0].friendRankOnes <= 0) {
    return null;
  }
  return rows[0].friendRankOnes - rows[1].friendRankOnes;
}

function seasonLeadMargin(rows: SeasonStandingsRowDto[]): number | null {
  if (rows.length < 2 || rows[0].boardsTaken <= 0) {
    return null;
  }
  return rows[0].boardsTaken - rows[1].boardsTaken;
}

function monthLabel(year: number, month: number) {
  return new Date(Date.UTC(year, month - 1, 1)).toLocaleString(undefined, {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  });
}

export function ChampionshipStandingsPanel({
  championship,
  memberCount,
  initialSeason,
  limit = 5,
}: Props) {
  const [view, setView] = useState<"lifetime" | "season">("lifetime");
  const [season, setSeason] = useState(initialSeason);
  const [pending, startTransition] = useTransition();

  if (memberCount < 2) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Podium className="size-4 shrink-0 text-[var(--accent-retro)]" />
            Friend championship
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Championship standings appear when your group has two or more members.
          </p>
        </CardContent>
      </Card>
    );
  }

  if (championship.length === 0 && season.rows.length === 0) {
    return null;
  }

  const lifetimeRows = championship.slice(0, limit);
  const seasonRows = season.rows.slice(0, limit);
  const lifetimeMargin = championshipLeadMargin(championship);
  const seasonMargin = seasonLeadMargin(season.rows);

  function onMonthChange(value: string) {
    const [y, m] = value.split("-").map(Number);
    if (!Number.isFinite(y) || !Number.isFinite(m)) {
      return;
    }
    startTransition(async () => {
      const result = await loadSeasonStandingsAction(y, m);
      if (result.ok) {
        setSeason(result.data);
      }
    });
  }

  return (
    <Card>
      <CardHeader className="space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle className="flex items-center gap-2">
            <Podium className="size-4 shrink-0 text-[var(--accent-retro)]" />
            Friend championship
          </CardTitle>
          <div className="flex rounded-md border border-border p-0.5 text-xs">
            <Button
              type="button"
              variant={view === "lifetime" ? "secondary" : "ghost"}
              size="sm"
              className="h-7 px-2.5"
              onClick={() => setView("lifetime")}
            >
              Lifetime
            </Button>
            <Button
              type="button"
              variant={view === "season" ? "secondary" : "ghost"}
              size="sm"
              className="h-7 px-2.5"
              onClick={() => setView("season")}
            >
              This month
            </Button>
          </div>
        </div>
        {view === "lifetime" && lifetimeMargin != null && lifetimeMargin > 0 ? (
          <p className="text-sm text-muted-foreground">
            <span className="font-mono text-foreground">+{lifetimeMargin}</span> board lead
            {lifetimeMargin === 1 ? "" : "s"} over 2nd
          </p>
        ) : null}
        {view === "season" ? (
          <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
            <span>{monthLabel(season.year, season.month)} (UTC)</span>
            {season.availableMonths.length > 1 ? (
              <select
                className="h-8 rounded-md bg-input px-2 text-xs outline-none disabled:opacity-50"
                value={`${season.year}-${season.month}`}
                disabled={pending}
                onChange={(event) => onMonthChange(event.target.value)}
              >
                {season.availableMonths.map((entry: SeasonMonthDto) => (
                  <option key={`${entry.year}-${entry.month}`} value={`${entry.year}-${entry.month}`}>
                    {monthLabel(entry.year, entry.month)}
                  </option>
                ))}
              </select>
            ) : null}
            {seasonMargin != null && seasonMargin > 0 ? (
              <span>
                · <span className="font-mono text-foreground">+{seasonMargin}</span> takes over 2nd
              </span>
            ) : null}
          </div>
        ) : null}
      </CardHeader>
      <CardContent className="px-0">
        {view === "lifetime" ? (
          <ul className="divide-y divide-border">
            {lifetimeRows.map((row, index) => (
              <li
                key={row.memberId}
                className="flex flex-col items-start px-(--card-spacing) py-1 text-sm sm:flex-row sm:items-center sm:justify-between sm:gap-4"
              >
                <div className="flex min-w-0 items-center">
                  <span className="w-6 font-mono text-xs text-muted-foreground">{index + 1}</span>
                  {index === 0 && row.friendRankOnes > 0 ? (
                    <Crown className="mr-2 size-4 shrink-0 text-[var(--accent-retro)]" />
                  ) : null}
                  <Link
                    href={`/members/${encodeURIComponent(row.raUsername)}`}
                    className={cn(
                      "flex min-w-0 items-center gap-1 font-medium hover:text-[var(--accent-retro)]",
                      index === 0 && row.friendRankOnes > 0 && "text-[var(--accent-retro)]",
                    )}
                  >
                    <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={24} />
                    <span className="truncate">{row.displayName}</span>
                  </Link>
                </div>
                <div className="shrink-0 whitespace-nowrap font-mono text-xs text-muted-foreground sm:text-right">
                  <span className="text-foreground">{row.friendRankOnes}</span> lead
                  {row.friendRankOnes === 1 ? "" : "s"}
                  <span className="mx-2 text-border">·</span>
                  {row.boardsWithScore} scored
                </div>
              </li>
            ))}
          </ul>
        ) : (
          <ul className="divide-y divide-border">
            {seasonRows.length === 0 ? (
              <li className="px-(--card-spacing) py-4 text-sm text-muted-foreground">
                No board takes recorded this month yet.
              </li>
            ) : (
              seasonRows.map((row, index) => (
                <li
                  key={row.memberId}
                  className="flex flex-col items-start px-(--card-spacing) py-1 text-sm sm:flex-row sm:items-center sm:justify-between sm:gap-4"
                >
                  <div className="flex min-w-0 items-center">
                    <span className="w-6 font-mono text-xs text-muted-foreground">{index + 1}</span>
                    {index === 0 && row.boardsTaken > 0 ? (
                      <Crown className="mr-2 size-4 shrink-0 text-[var(--accent-retro)]" />
                    ) : null}
                    <Link
                      href={`/members/${encodeURIComponent(row.raUsername)}`}
                      className={cn(
                        "flex min-w-0 items-center gap-1 font-medium hover:text-[var(--accent-retro)]",
                        index === 0 && row.boardsTaken > 0 && "text-[var(--accent-retro)]",
                      )}
                    >
                      <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={24} />
                      <span className="truncate">{row.displayName}</span>
                    </Link>
                  </div>
                  <div className="shrink-0 font-mono text-xs text-muted-foreground">
                    <span className="text-foreground">{row.boardsTaken}</span> board
                    {row.boardsTaken === 1 ? "" : "s"} taken
                  </div>
                </li>
              ))
            )}
          </ul>
        )}
      </CardContent>
      <CardFooter className="flex flex-wrap gap-3 border-t py-3">
        {view === "season" ? (
          <Link
            href={`/summary/${season.year}/${season.month}`}
            className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
          >
            Full month summary
          </Link>
        ) : null}
        {view === "lifetime" ? (
          <Link
            href={`/summary/${new Date().getUTCFullYear()}/${new Date().getUTCMonth() + 1}`}
            className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
          >
            This month&apos;s summary
          </Link>
        ) : null}
        {view === "lifetime" && championship.length >= 2 ? (
          <Link
            href={rivalryPath(championship[0].raUsername, championship[1].raUsername)}
            className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]"
          >
            #1 vs #2 rivalry
          </Link>
        ) : null}
        <Link href="/rivalry" className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]">
          Head-to-head hub
        </Link>
        <Link href="/members" className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]">
          All members
        </Link>
      </CardFooter>
    </Card>
  );
}
