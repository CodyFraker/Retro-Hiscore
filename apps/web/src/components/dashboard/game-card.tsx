import { ChevronRight, ImageOff, Trash2, TableProperties, Trophy, Users } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { ConsoleName } from "@/components/console-name";
import { GameCardPlayerAvatars } from "@/components/dashboard/game-card-player-avatars";
import { Button } from "@/components/ui/button";
import type { DashboardGameDto } from "@/generated/api-client";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { LastSyncedLabel } from "@/components/sync/last-synced-label";
import { LeaderboardSyncTierBadge } from "@/components/sync/leaderboard-sync-tier-badge";

type Props = {
  game: DashboardGameDto;
  onDelete?: () => void;
  deleting?: boolean;
};

export function GameCard({ game, onDelete, deleting }: Props) {
  const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;

  return (
    <li>
      <div className="flex items-stretch gap-2">
        <Link
          href={`/games/${game.raGameId}`}
          className="flex min-w-0 flex-1 items-center py-5 pl-4 transition-colors hover:bg-secondary/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
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
              
              <div className="mt-1 flex flex-wrap items-center gap-2">
              <p className="truncate text-xl font-medium">{game.title}</p>
                <LeaderboardSyncTierBadge status={game.leaderboardSyncStatus} />
              </div>
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
          </div>
          <ChevronRight
            className="mr-2 hidden size-8 shrink-0 text-muted-foreground sm:block"
            aria-hidden
          />
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
      <div className="flex items-center justify-between gap-4 px-4 pb-4 pt-1 max-h-8">
        <div className="min-w-0 flex-1">
          <div className="mb-2 flex items-center gap-2 text-sm text-muted-foreground">
            <span className="shrink-0">Players:</span>
            <GameCardPlayerAvatars players={game.playersWithAvatars} />
          </div>
        </div>
        <div className="flex shrink-0 flex-wrap items-center justify-end gap-1.5">
          <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground flex items-center gap-1">
            <TableProperties className="size-4 mr-1" />
            {game.leaderboardCount === 0
              ? "Waiting for first sync"
              : `${game.leaderboardCount} boards`}
          </span>
          {game.totalRankedEntriesAcrossBoards != null && (
            <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground flex items-center gap-1">
              <Users className="size-4 mr-1" /> {game.totalRankedEntriesAcrossBoards.toLocaleString()} Entries
            </span>
          )}
          {game.totalAchievementsInCatalog != null && (
            <span className="rounded-md bg-secondary px-2 py-0.5 text-xs text-secondary-foreground flex items-center gap-1">
              <Trophy className="size-4 mr-1" /> {game.totalAchievementsInCatalog} achievements
            </span>
          )}
        </div>
      </div>
    </li>
  );
}
