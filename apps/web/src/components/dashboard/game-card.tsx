import { ChevronRight, ImageOff, Trash2 } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { ConsoleName } from "@/components/console-name";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { DashboardGameDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";

type Props = {
  game: DashboardGameDto;
  recentChangeCount?: number;
  onDelete?: () => void;
  deleting?: boolean;
};

export function GameCard({ game, recentChangeCount = 0, onDelete, deleting }: Props) {
  const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;
  const recentlyActive = recentChangeCount > 0;

  return (
    <li
      className={
        recentlyActive
          ? "border-l-2 border-[var(--accent-retro)]"
          : undefined
      }
    >
      <div className="flex items-center gap-2 pr-2">
        <Link
          href={`/games/${game.raGameId}`}
          className="flex min-w-0 flex-1 items-center gap-4 py-5 pl-4 transition-colors hover:bg-secondary/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
        >
          <div className="relative h-16 w-16 shrink-0 overflow-hidden rounded border border-border bg-secondary/40">
            {artUrl ? (
              <Image
                src={artUrl}
                alt={game.title}
                fill
                className="object-cover"
                sizes="64px"
              />
            ) : (
              <div className="flex h-full items-center justify-center text-muted-foreground">
                <ImageOff className="size-5" />
              </div>
            )}
          </div>
          <div className="min-w-0 flex-1">
            <p className="truncate text-xl font-medium">{game.title}</p>
            <p className="mt-1">
              <ConsoleName
                name={game.consoleName}
                iconUrl={game.consoleIconUrl}
                fallback={`RA #${game.raGameId}`}
              />
            </p>
            {game.lastActivityAt && (
              <p className="mt-0.5 text-xs text-muted-foreground">
                Last activity {formatSyncTime(game.lastActivityAt)}
              </p>
            )}
          </div>
          <div className="flex max-sm:w-full max-sm:items-start max-sm:pt-1 shrink-0 flex-wrap flex-col items-end gap-1.5 sm:flex-row sm:items-center">
            {recentlyActive && (
              <Badge className="bg-[var(--accent-retro)]/15 text-[var(--accent-retro)] hover:bg-[var(--accent-retro)]/15">
                {recentChangeCount} change{recentChangeCount === 1 ? "" : "s"}
              </Badge>
            )}
            <Badge variant="secondary">
              {game.leaderboardCount === 0
                ? "Waiting for first sync"
                : `${game.leaderboardCount} boards`}
            </Badge>
          </div>
          <ChevronRight className="hidden size-4 shrink-0 text-muted-foreground sm:block" aria-hidden />
        </Link>
        {onDelete && (
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            className="min-h-11 min-w-11 sm:min-h-0 sm:min-w-0"
            disabled={deleting}
            aria-label={`Stop tracking ${game.title}`}
            onClick={onDelete}
          >
            <Trash2 className="size-4" />
          </Button>
        )}
      </div>
      {game.friendRankOneLeader && (
        <p className="px-4 pb-4 text-xs text-muted-foreground">
          <Link
            href={`/members/${encodeURIComponent(game.friendRankOneLeader.raUsername)}`}
            className="font-medium text-foreground hover:text-[var(--accent-retro)]"
          >
            {game.friendRankOneLeader.displayName}
          </Link>{" "}
          leads {game.friendRankOneLeader.friendRankOnes} board
          {game.friendRankOneLeader.friendRankOnes === 1 ? "" : "s"}
        </p>
      )}
    </li>
  );
}
