import Link from "next/link";

type Props = {
  raGameId: number;
  title: string;
  isTracked: boolean;
  className?: string;
};

export function GameOfTheWeekWinnerLink({ raGameId, title, isTracked, className }: Props) {
  const linkClass = className ?? "font-medium hover:text-[var(--accent-retro)]";

  if (isTracked) {
    return (
      <Link href={`/games/${raGameId}`} className={linkClass}>
        {title}
      </Link>
    );
  }

  return (
    <a
      href={`https://retroachievements.org/game/${raGameId}`}
      target="_blank"
      rel="noopener noreferrer"
      className={linkClass}
    >
      {title}
    </a>
  );
}
