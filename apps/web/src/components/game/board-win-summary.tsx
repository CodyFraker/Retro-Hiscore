import Link from "next/link";
import type { BoardWinSummaryRow } from "@/lib/board-wins";

type Props = {
  rows: BoardWinSummaryRow[];
};

export function BoardWinSummary({ rows }: Props) {
  if (rows.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h2 className="text-lg font-medium">Board leads</h2>
      <ul className="divide-y divide-border border-y border-border">
        {rows.map((row) => (
          <li key={row.memberId} className="flex items-center justify-between gap-4 py-3 text-sm">
            <Link
              href={`/members/${encodeURIComponent(row.raUsername)}`}
              className="font-medium hover:text-[var(--accent-retro)]"
            >
              {row.displayName}
            </Link>
            <div className="font-mono text-xs text-muted-foreground">
              <span className="text-foreground">{row.friendRankOnes}</span> lead
              {row.friendRankOnes === 1 ? "" : "s"}
              <span className="mx-2 text-border">·</span>
              {row.boardsWithScore} scored
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
