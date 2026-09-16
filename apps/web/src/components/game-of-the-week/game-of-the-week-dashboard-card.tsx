import Link from "next/link";
import type { GameOfTheWeekCurrentPollDto } from "@/generated/api-client";

const PHASE_SCHEDULED = 0;
const PHASE_OPEN = 1;

type Props = {
  poll: GameOfTheWeekCurrentPollDto;
};

export function GameOfTheWeekDashboardCard({ poll }: Props) {
  if (poll.phase !== PHASE_SCHEDULED && poll.phase !== PHASE_OPEN) {
    return null;
  }

  const label = poll.phase === PHASE_OPEN ? "Vote now" : "Opens soon";

  return (
    <section className="rounded border border-border bg-card p-5">
      <h2 className="text-lg font-semibold">Game of the week</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        {poll.ballot.length} on the ballot · {label}
      </p>
      <Link
        href="/game-of-the-week"
        className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
      >
        Go to voting →
      </Link>
    </section>
  );
}
