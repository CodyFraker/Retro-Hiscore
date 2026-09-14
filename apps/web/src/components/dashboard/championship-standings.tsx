import { Crown } from "lucide-react";
import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { ChampionshipRowDto } from "@/generated/api-client";

type Props = {
  rows: ChampionshipRowDto[];
  limit?: number;
};

export function ChampionshipStandings({ rows, limit = 5 }: Props) {
  if (rows.length === 0) {
    return null;
  }

  const visibleRows = rows.slice(0, limit);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Friend championship</CardTitle>
      </CardHeader>
      <CardContent className="px-0">
        <ul className="divide-y divide-border">
          {visibleRows.map((row, index) => (
            <li
              key={row.memberId}
              className="flex flex-col items-start gap-1 px-(--card-spacing) py-3 text-sm sm:flex-row sm:items-center sm:justify-between sm:gap-4"
            >
              <div className="flex items-center gap-3">
                <span className="w-6 font-mono text-xs text-muted-foreground">{index + 1}</span>
                {index === 0 && row.friendRankOnes > 0 && (
                  <Crown className="size-4 shrink-0 text-[var(--accent-retro)]" />
                )}
                <Link
                  href={`/members/${encodeURIComponent(row.raUsername)}`}
                  className={`flex items-center gap-2 font-medium hover:text-[var(--accent-retro)] ${index === 0 && row.friendRankOnes > 0 ? "text-[var(--accent-retro)]" : ""}`}
                >
                  <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={24} />
                  {row.displayName}
                </Link>
              </div>
              <div className="font-mono text-xs text-muted-foreground sm:text-right">
                <span className="text-foreground">{row.friendRankOnes}</span> lead
                {row.friendRankOnes === 1 ? "" : "s"}
                <span className="mx-2 text-border">·</span>
                {row.boardsWithScore} scored
              </div>
            </li>
          ))}
        </ul>
      </CardContent>
      <CardFooter className="border-t">
        <Link href="/members" className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]">
          See all members
        </Link>
      </CardFooter>
    </Card>
  );
}
