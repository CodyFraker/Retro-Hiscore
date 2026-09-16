import { ImageOff } from "lucide-react";
import Image from "next/image";
import { ConsoleName } from "@/components/console-name";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Badge } from "@/components/ui/badge";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { MemberRaUnlockedAchievementBadges } from "@/components/members/member-ra-unlocked-achievement-badges";
import type { MemberRaRecentGameDto } from "@/generated/api-client";

type Props = {
  games: MemberRaRecentGameDto[];
  excludeGameId?: number;
  pendingRaGameIds?: ReadonlySet<number>;
};

export function MemberRaRecentlyPlayed({ games, excludeGameId, pendingRaGameIds }: Props) {
  const items = games.filter((g) => g.raGameId !== excludeGameId);
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h3 className="steam-section-heading">Recently played</h3>
      <ul className="flex gap-3 overflow-x-auto pb-1">
        {items.map((game) => {
          const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;
          return (
            <li
              key={game.raGameId}
              className="w-44 shrink-0 rounded-md border border-border bg-card p-2"
            >
              <MemberRaGameLink
                raGameId={game.raGameId}
                isTracked={game.isTracked}
                trackQueuePending={pendingRaGameIds?.has(game.raGameId) ?? false}
                className="block space-y-2"
              >
                <div className="relative aspect-square overflow-hidden rounded bg-secondary/40">
                  {artUrl ? (
                    <Image src={artUrl} alt="" fill className="object-cover" sizes="176px" />
                  ) : (
                    <div className="flex h-full items-center justify-center text-muted-foreground">
                      <ImageOff className="size-5" />
                    </div>
                  )}
                </div>
                <p className="line-clamp-2 text-sm font-medium leading-snug">{game.title}</p>
              </MemberRaGameLink>
              {!game.isTracked && pendingRaGameIds?.has(game.raGameId) ? (
                <Badge variant="outline" className="mt-1 text-xs">Requested for tracking</Badge>
              ) : null}
              <ConsoleName
                name={game.consoleName}
                iconUrl={game.consoleIconUrl}
                fallback={`#${game.consoleId}`}
              />
              {game.lastPlayedAt ? (
                <p className="mt-1 text-xs text-muted-foreground">
                  <FormattedSyncTime value={game.lastPlayedAt} />
                </p>
              ) : null}
              {game.unlockedAchievements.length > 0 ? (
                <div className="mt-2">
                  <MemberRaUnlockedAchievementBadges
                    achievements={game.unlockedAchievements}
                    size="sm"
                    maxVisible={6}
                  />
                </div>
              ) : null}
            </li>
          );
        })}
      </ul>
    </section>
  );
}
