import type { GameOfTheWeekVoteCastDto } from "@/generated/api-client";

type Props = {
  voters: GameOfTheWeekVoteCastDto[];
  currentMemberId?: string | null;
  compact?: boolean;
  maxNames?: number;
};

export function GameOfTheWeekVoterList({
  voters,
  currentMemberId,
  compact = false,
  maxNames = 3,
}: Props) {
  if (voters.length === 0) {
    return null;
  }

  if (compact) {
    const names = voters.map((v) =>
      currentMemberId && v.memberId === currentMemberId ? `${v.displayName} (you)` : v.displayName,
    );
    const visible = names.slice(0, maxNames);
    const remaining = names.length - visible.length;
    const label =
      remaining > 0 ? `${visible.join(", ")} +${remaining}` : visible.join(", ");

    return <p className="text-xs text-muted-foreground">{label}</p>;
  }

  return (
    <ul className="mt-2 flex flex-wrap gap-1.5">
      {voters.map((voter) => {
        const isYou = currentMemberId != null && voter.memberId === currentMemberId;
        return (
          <li
            key={voter.memberId}
            className={
              isYou
                ? "rounded-full border border-[var(--accent-retro)]/50 bg-[var(--accent-retro)]/10 px-2 py-0.5 text-xs font-medium"
                : "rounded-full border border-border bg-muted/40 px-2 py-0.5 text-xs text-muted-foreground"
            }
          >
            {voter.displayName}
            {isYou ? " (you)" : null}
          </li>
        );
      })}
    </ul>
  );
}
