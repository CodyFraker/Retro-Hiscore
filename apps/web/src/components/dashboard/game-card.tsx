import { ChevronRight, ImageOff, Trash2 } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { ConsoleName } from "@/components/console-name";
import { GameCardPlayerAvatars } from "@/components/dashboard/game-card-player-avatars";
import { Button } from "@/components/ui/button";
import type { DashboardGameDto } from "@/generated/api-client";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";

type Props = {
  game: DashboardGameDto;
  onDelete?: () => void;
  deleting?: boolean;
};

export function GameCard({ game, onDelete, deleting }: Props) {
  const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;

  return (
    <li>
      <div className="flex items-stretch gap-2 pr-2">
        <Link
          href={`/games/${game.raGameId}`}
          className="flex min-w-0 flex-1 py-5 pl-4 transition-colors hover:bg-secondary/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
        >
          <div className="flex min-w-0 flex-1 items-start gap-4">
            <div className="relative h-16 w-16 shrink-0 overflow-hidden rounded-md bg-secondary/40 steam-bevel-inset">
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
              {game.leaderboardScoresSyncedAt ? (
                <div className="mt-0.5">
                  <LastSyncedLabel
                    at={game.leaderboardScoresSyncedAt}
                    prefix="Scores last synced"
                  />
                </div>
              ) : game.lastActivityAt ? (
                <p className="mt-0.5 text-xs text-muted-foreground">
                  Last activity <FormattedSyncTime value={game.lastActivityAt} />
                </p>
              ) : null}
            </div>
            <ChevronRight
              className="hidden size-4 shrink-0 text-muted-foreground sm:block"
              aria-hidden
            />
          </div>
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
      <div className="flex items-center justify-between gap-4 px-4 pb-4 pt-1">
        <div className="min-w-0 flex-1">
          <GameCardPlayerAvatars players={game.playersWithAvatars} />
        </div>
        <div className="flex shrink-0 flex-wrap items-center justify-end gap-1.5">
          <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground">
            {game.leaderboardCount === 0
              ? "Waiting for first sync"
              : `${game.leaderboardCount} boards`}
          </span>
          {game.maxGlobalEntryCount != null && (
            <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground">
              {game.maxGlobalEntryCount.toLocaleString()} Entries
            </span>
          )}
          {game.totalAchievementsInCatalog != null && (
            <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground">
              {game.totalAchievementsInCatalog} achievements
            </span>
          )}
        </div>
      </div>
    </li>
  );
}
