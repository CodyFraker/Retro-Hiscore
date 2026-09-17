import { Crown, Podium } from "lucide-react";
import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { ChampionshipRowDto } from "@/generated/api-client";
import { rivalryPath } from "@/lib/rivalry-path";

type Props = {
  rows: ChampionshipRowDto[];
  memberCount: number;
  limit?: number;
};

function championshipLeadMargin(rows: ChampionshipRowDto[]): number | null {
  if (rows.length < 2 || rows[0].friendRankOnes <= 0) {
    return null;
  }
  return rows[0].friendRankOnes - rows[1].friendRankOnes;
}

export function ChampionshipStandings({ rows, memberCount, limit = 5 }: Props) {
  if (memberCount < 2) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Friend championship</CardTitle>
          <Podium className="size-4 shrink-0 text-[var(--accent-retro)]" />
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Championship standings appear when your group has two or more members.
          </p>
        </CardContent>
      </Card>
    );
  }

  if (rows.length === 0) {
    return null;
  }

  const visibleRows = rows.slice(0, limit);
  const margin = championshipLeadMargin(rows);

  return (
    <Card>
      <CardHeader className="space-y-1">
        <CardTitle className="flex items-center gap-2">
          <Podium className="size-4 shrink-0 text-[var(--accent-retro)]" />
          Friend championship
        </CardTitle>
        
        {margin != null && margin > 0 ? (
          <p className="text-sm text-muted-foreground">
            <span className="font-mono text-foreground">+{margin}</span> board lead
            {margin === 1 ? "" : "s"} over 2nd
          </p>
        ) : null}
      </CardHeader>
      <CardContent className="px-0">
        <ul className="divide-y divide-border">
          {visibleRows.map((row, index) => (
            <li
              key={row.memberId}
              className="flex flex-col items-start px-(--card-spacing) py-1 text-sm sm:flex-row sm:items-center sm:justify-between sm:gap-4"
            >
              <div className="flex min-w-0 items-center">
                <span className="w-6 font-mono text-xs text-muted-foreground">{index + 1}</span>
                {index === 0 && row.friendRankOnes > 0 && (
                  <Crown className="size-4 shrink-0 mr-2 text-[var(--accent-retro)]" />
                )}
                <Link
                  href={`/members/${encodeURIComponent(row.raUsername)}`}
                  className={`flex min-w-0 items-center gap-1 font-medium hover:text-[var(--accent-retro)] ${index === 0 && row.friendRankOnes > 0 ? "text-[var(--accent-retro)]" : ""}`}
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
      </CardContent>
      <CardFooter className="flex flex-wrap gap-3 border-t max-h-none py-3">
        {rows.length >= 2 ? (
          <Link
            href={rivalryPath(rows[0].raUsername, rows[1].raUsername)}
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
