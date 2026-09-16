import Link from "next/link";
import type { ReactNode } from "react";

type Props = {
  raGameId: number;
  isTracked: boolean;
  trackQueuePending?: boolean;
  className?: string;
  children: ReactNode;
};

export function MemberRaGameLink({
  raGameId,
  isTracked,
  trackQueuePending = false,
  className,
  children,
}: Props) {
  if (isTracked) {
    return (
      <Link href={`/games/${raGameId}`} className={className}>
        {children}
      </Link>
    );
  }

  if (trackQueuePending) {
    return (
      <Link href="/games#pending-track" className={className}>
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
