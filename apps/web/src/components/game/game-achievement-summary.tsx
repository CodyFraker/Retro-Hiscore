import type { GameAchievementMemberDto, GameAchievementMemberSummaryDto } from "@/generated/api-client";

type Props = {
  members: GameAchievementMemberDto[];
  summaries: GameAchievementMemberSummaryDto[];
  membersMastered: number;
};

export function GameAchievementSummary({ members, summaries, membersMastered }: Props) {
  const memberById = new Map(members.map((m) => [m.memberId, m]));
  const rows = summaries
    .filter((s) => s.achievementsTotal > 0)
    .sort((a, b) => b.achievementsEarned - a.achievementsEarned || b.pointsEarned - a.pointsEarned);

  if (rows.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No achievement progress synced yet. Run member achievement sync, rank sync, or refresh scores on
        this game.
      </p>
    );
  }

  return (
    <div className="space-y-4">
      {membersMastered > 0 && (
        <p className="text-sm text-muted-foreground">
          <span className="font-mono text-foreground">{membersMastered}</span> friend
          {membersMastered === 1 ? "" : "s"} mastered this set.
        </p>
      )}
      <ul className="divide-y divide-border rounded-md border border-border">
        {rows.map((summary) => {
          const member = memberById.get(summary.memberId);
          const pct = Math.min(
            100,
            (summary.achievementsEarned / summary.achievementsTotal) * 100,
          );
          return (
            <li key={summary.memberId} className="space-y-2 px-4 py-3">
              <div className="flex justify-between gap-2 text-sm">
                <span className="font-medium">{member?.displayName ?? "Member"}</span>
                <span className="font-mono tabular-nums text-muted-foreground">
                  {summary.achievementsEarned}/{summary.achievementsTotal}
                  <span className="text-foreground">
                    {" "}
                    · {summary.pointsEarned}/{summary.pointsPossible} pts
                  </span>
                </span>
              </div>
              <div className="h-1.5 overflow-hidden rounded-full bg-secondary">
                <div
                  className="h-full rounded-full bg-[var(--accent-retro)]"
                  style={{ width: `${pct}%` }}
                />
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
