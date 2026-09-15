import Link from "next/link";
import type { MemberAchievementListItemDto } from "@/generated/api-client";

type Props = {
  items: MemberAchievementListItemDto[];
};

export function MemberTrackedAchievementsTable({ items }: Props) {
  const byGame = new Map<number, { title: string; count: number; points: number }>();
  for (const item of items) {
    if (!item.isTracked) continue;
    const existing = byGame.get(item.raGameId);
    if (existing) {
      existing.count += 1;
      existing.points += item.points;
    } else {
      byGame.set(item.raGameId, { title: item.gameTitle, count: 1, points: item.points });
    }
  }

  const rows = [...byGame.entries()].sort((a, b) => b[1].count - a[1].count);

  if (rows.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No unlocks in tracked games yet. Achievement sync runs on rank sync and manual jobs.
      </p>
    );
  }

  return (
    <div className="md:overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-border text-left text-muted-foreground">
            <th className="py-2 pr-4 font-medium">Game</th>
            <th className="py-2 font-medium">Unlocks</th>
            <th className="py-2 font-medium">Points</th>
          </tr>
        </thead>
        <tbody>
          {rows.map(([raGameId, row]) => (
            <tr key={raGameId} className="border-b border-border/60">
              <td className="py-2 pr-4">
                <Link
                  href={`/games/${raGameId}`}
                  className="font-medium hover:text-[var(--accent-retro)]"
                >
                  {row.title}
                </Link>
              </td>
              <td className="py-2 font-mono tabular-nums">{row.count}</td>
              <td className="py-2 font-mono tabular-nums">{row.points}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
