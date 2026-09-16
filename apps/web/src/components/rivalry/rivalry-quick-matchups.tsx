import Link from "next/link";
import type { ChampionshipRowDto } from "@/generated/api-client";
import { rivalryPath } from "@/lib/rivalry-path";

type Props = {
  championship: ChampionshipRowDto[];
};

export function RivalryQuickMatchups({ championship }: Props) {
  if (championship.length < 2) {
    return null;
  }

  const first = championship[0];
  const second = championship[1];

  return (
    <section className="rounded border border-border bg-card p-5">
      <h2 className="text-lg font-semibold">Quick matchup</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Championship leaders side by side on every tracked board.
      </p>
      <Link
        href={rivalryPath(first.raUsername, second.raUsername)}
        className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
      >
        {first.displayName} vs {second.displayName} →
      </Link>
    </section>
  );
}
