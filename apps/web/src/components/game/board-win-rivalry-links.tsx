import Link from "next/link";
import type { BoardWinSummaryRow } from "@/lib/board-wins";
import { rivalryPath } from "@/lib/rivalry-path";

type Props = {
  rows: BoardWinSummaryRow[];
  currentRaUsername: string | null | undefined;
  memberCount: number;
};

export function BoardWinRivalryLinks({ rows, currentRaUsername, memberCount }: Props) {
  if (memberCount < 2 || !currentRaUsername?.trim()) {
    return null;
  }

  const others = rows.filter((row) => row.raUsername !== currentRaUsername);
  if (others.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-x-3 gap-y-1 text-sm">
      {others.map((row) => (
        <Link
          key={row.memberId}
          href={rivalryPath(currentRaUsername, row.raUsername)}
          className="text-muted-foreground hover:text-[var(--accent-retro)]"
        >
          You vs {row.displayName}
        </Link>
      ))}
    </div>
  );
}
