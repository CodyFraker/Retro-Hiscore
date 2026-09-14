import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
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
      <h2 className="steam-section-heading">Board leads</h2>
      <ul className="divide-y divide-border border-y border-border">
        {rows.map((row) => (
          <li key={row.memberId} className="flex items-center justify-between gap-4 py-3 text-sm">
            <Link
              href={`/members/${encodeURIComponent(row.raUsername)}`}
              className="flex items-center gap-2 font-medium hover:text-[var(--accent-retro)]"
            >
              <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={24} />
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
