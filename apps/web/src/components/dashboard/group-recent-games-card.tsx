import { EyeOff, ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { ConsoleName } from "@/components/console-name";
import { GameCardPlayerAvatars } from "@/components/dashboard/game-card-player-avatars";
import { RaGameModBadges } from "@/components/game/ra-game-mod-badges";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RequestGameTrackingButton } from "@/components/games/request-game-tracking-button";
import type { RecentGroupGameDto } from "@/generated/api-client";
import { parseRaGameTitle } from "@/lib/ra-game-title";

type Props = {
  games: RecentGroupGameDto[];
  isAdmin?: boolean;
};

const gameArtClassName =
  "relative h-14 w-14 shrink-0 overflow-hidden rounded-md bg-secondary/40";

export function GroupRecentGamesCard({ games, isAdmin = false }: Props) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle>Recently played</CardTitle>
      </CardHeader>
      <CardContent>
        {games.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            Shows up after the next score sync.
          </p>
        ) : (
          <ul className="divide-y divide-border">
            {games.map((game) => {
              const { displayTitle, modTags } = parseRaGameTitle(game.title);
              const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;
              const art = artUrl ? (
                <Image src={artUrl} alt="" fill className="object-cover" sizes="56px" />
              ) : (
                <div className="flex h-full items-center justify-center text-muted-foreground">
                  <ImageOff className="size-4" />
                </div>
              );

              return (
                <li key={game.raGameId} className="flex gap-3 py-3 first:pt-0 last:pb-0">
                  {game.isTracked ? (
                    <Link href={`/games/${game.raGameId}`} className={gameArtClassName}>
                      {art}
                    </Link>
                  ) : (
                    <div className={gameArtClassName}>{art}</div>
                  )}
                  <div className="min-w-0 flex-1 space-y-1">
                    <div className="flex min-w-0 items-start gap-1.5">
                      <div className="min-w-0 flex-1">
                        {game.isTracked ? (
                          <Link
                            href={`/games/${game.raGameId}`}
                            className="line-clamp-2 text-sm font-medium leading-snug hover:text-[var(--accent-retro)]"
                          >
                            {displayTitle}
                          </Link>
                        ) : (
                          <p
                            className="flex items-start gap-1.5 text-sm font-medium leading-snug text-foreground cursor-default"
                            title="Not tracked yet"
                          >
                            <EyeOff
                              className="mt-0.5 size-3.5 shrink-0 text-muted-foreground"
                              aria-hidden
                            />
                            <span className="line-clamp-2 min-w-0">{displayTitle}</span>
                          </p>
                        )}
                      </div>
                      <RaGameModBadges modTags={modTags} />
                    </div>
                    <ConsoleName
                      name={game.consoleName}
                      iconUrl={game.consoleIconUrl}
                      fallback={`#${game.consoleId}`}
                    />
                    <p className="text-xs text-muted-foreground">
                      <FormattedSyncTime value={game.lastPlayedAt} />
                    </p>
                    {!game.isTracked ? (
                      <div className="flex flex-wrap items-center gap-2 pt-1">
                        {isAdmin ? (
                          <Link
                            href="/admin/games"
                            className="text-xs text-foreground underline-offset-4 hover:underline"
                          >
                            Review in game track queue
                          </Link>
                        ) : (
                          <RequestGameTrackingButton raGameId={game.raGameId} title={displayTitle} />
                        )}
                      </div>
                    ) : null}
                    {game.players.length > 0 ? (
                      <GameCardPlayerAvatars
                        players={game.players.map((p) => ({
                          raUsername: p.raUsername,
                          displayName: p.displayName,
                          avatarUrl: p.avatarUrl ?? "",
                        }))}
                      />
                    ) : null}
                  </div>
                </li>
              );
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
