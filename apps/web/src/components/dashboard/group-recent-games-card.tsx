import { ImageOff } from "lucide-react";
import Image from "next/image";
import { ConsoleName } from "@/components/console-name";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { RecentGroupGameDto } from "@/generated/api-client";

type Props = {
  games: RecentGroupGameDto[];
};

export function GroupRecentGamesCard({ games }: Props) {
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
              const artUrl = game.imageBoxArtUrl ?? game.imageIconUrl;
              return (
                <li key={game.raGameId} className="flex gap-3 py-3 first:pt-0 last:pb-0">
                  <MemberRaGameLink
                    raGameId={game.raGameId}
                    isTracked={game.isTracked}
                    className="relative h-14 w-14 shrink-0 overflow-hidden rounded-md bg-secondary/40"
                  >
                    {artUrl ? (
                      <Image src={artUrl} alt="" fill className="object-cover" sizes="56px" />
                    ) : (
                      <div className="flex h-full items-center justify-center text-muted-foreground">
                        <ImageOff className="size-4" />
                      </div>
                    )}
                  </MemberRaGameLink>
                  <div className="min-w-0 flex-1 space-y-1">
                    <MemberRaGameLink
                      raGameId={game.raGameId}
                      isTracked={game.isTracked}
                      className="line-clamp-2 text-sm font-medium leading-snug hover:text-[var(--accent-retro)]"
                    >
                      {game.title}
                    </MemberRaGameLink>
                    <ConsoleName
                      name={game.consoleName}
                      iconUrl={game.consoleIconUrl}
                      fallback={`#${game.consoleId}`}
                    />
                    <p className="text-xs text-muted-foreground">
                      <FormattedSyncTime value={game.lastPlayedAt} />
                    </p>
                    {game.players.length > 0 ? (
                      <p className="text-xs text-muted-foreground">
                        {game.players.map((p) => p.displayName).join(", ")}
                      </p>
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
