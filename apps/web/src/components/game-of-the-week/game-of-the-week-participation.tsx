import type { GameOfTheWeekCurrentPollDto } from "@/generated/api-client";

type Props = {
  poll: GameOfTheWeekCurrentPollDto;
};

export function GameOfTheWeekParticipation({ poll }: Props) {
  const { votesCastCount, eligibleVoterCount } = poll;
  if (eligibleVoterCount <= 0) {
    return null;
  }

  const ratio = Math.min(1, votesCastCount / eligibleVoterCount);

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between text-xs text-muted-foreground">
        <span>Votes cast</span>
        <span>
          {votesCastCount} / {eligibleVoterCount}
        </span>
      </div>
      <div className="h-1.5 overflow-hidden rounded-full bg-muted">
        <div
          className="h-full rounded-full bg-[var(--accent-retro)] transition-[width]"
          style={{ width: `${ratio * 100}%` }}
        />
      </div>
    </div>
  );
}
