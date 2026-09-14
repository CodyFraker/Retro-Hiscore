import Link from "next/link";
import type { ReactNode } from "react";

type Props = {
  raGameId: number;
  isTracked: boolean;
  className?: string;
  children: ReactNode;
};

export function MemberRaGameLink({ raGameId, isTracked, className, children }: Props) {
  if (isTracked) {
    return (
      <Link href={`/games/${raGameId}`} className={className}>
        {children}
      </Link>
    );
  }

  return (
    <a
      href={`https://retroachievements.org/game/${raGameId}`}
      target="_blank"
      rel="noopener noreferrer"
      className={className}
    >
      {children}
    </a>
  );
}
