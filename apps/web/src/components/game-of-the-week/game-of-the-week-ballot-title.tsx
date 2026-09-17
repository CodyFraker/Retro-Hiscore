import Link from "next/link";

type Props = {
  raGameId: number;
  title: string;
  isTracked: boolean;
  className?: string;
};

export function GameOfTheWeekBallotTitle({ raGameId, title, isTracked, className }: Props) {
  const baseClass = className ?? "font-medium hover:text-[var(--accent-retro)]";

  if (isTracked) {
    return (
      <Link href={`/games/${raGameId}`} className={baseClass}>
        {title}
      </Link>
    );
  }

  return (
    <a
      href={`https://retroachievements.org/game/${raGameId}`}
      target="_blank"
      rel="noopener noreferrer"
      className={baseClass}
    >
      {title}
    </a>
  );
}
