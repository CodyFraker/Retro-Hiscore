import Link from "next/link";
import {
  Avatar,
  AvatarFallback,
  AvatarGroup,
  AvatarGroupCount,
  AvatarImage,
} from "@/components/ui/avatar";
import type { DashboardGamePlayerAvatarDto } from "@/generated/api-client";

const MAX_VISIBLE = 4;

function avatarFallbackInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length >= 2) {
    return (parts[0]![0]! + parts[1]![0]!).toUpperCase();
  }
  return name.slice(0, 2).toUpperCase();
}

type Props = {
  players: DashboardGamePlayerAvatarDto[];
};

export function GameCardPlayerAvatars({ players }: Props) {
  if (players.length === 0) {
    return null;
  }

  const visible = players.slice(0, MAX_VISIBLE);
  const overflow = players.length - visible.length;

  return (
    <div className="px-4 pb-4 pt-1">
      <AvatarGroup>
        {visible.map((player) => (
          <Link
            key={player.raUsername}
            href={`/members/${encodeURIComponent(player.raUsername)}`}
            title={player.displayName}
            className="rounded-full transition-opacity hover:opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
          >
            <Avatar size="sm">
              <AvatarImage src={player.avatarUrl} alt={player.displayName} />
              <AvatarFallback>{avatarFallbackInitials(player.displayName)}</AvatarFallback>
            </Avatar>
          </Link>
        ))}
        {overflow > 0 && <AvatarGroupCount>+{overflow}</AvatarGroupCount>}
      </AvatarGroup>
    </div>
  );
}
