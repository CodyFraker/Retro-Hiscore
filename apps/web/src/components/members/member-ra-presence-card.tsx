import { ImageOff } from "lucide-react";
import Image from "next/image";
import { ConsoleName } from "@/components/console-name";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { MemberRaUnlockedAchievementBadges } from "@/components/members/member-ra-unlocked-achievement-badges";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { MemberRaPresenceGameDto } from "@/generated/api-client";

type Props = {
  presence: MemberRaPresenceGameDto;
};

function AchievementProgress({ presence }: { presence: MemberRaPresenceGameDto }) {
  const progress = presence.progress;
  if (!progress || progress.achievementsTotal <= 0) {
    return null;
  }

  const pct = Math.min(100, (progress.achievementsEarned / progress.achievementsTotal) * 100);

  return (
    <div className="space-y-1.5">
      <div className="flex justify-between text-xs text-muted-foreground">
        <span>Achievements</span>
        <span className="font-mono tabular-nums text-foreground">
          {progress.achievementsEarned}/{progress.achievementsTotal}
          {progress.pointsPossible != null ? (
            <span className="text-muted-foreground">
              {" "}
              · {progress.pointsEarned}/{progress.pointsPossible} pts
            </span>
          ) : null}
        </span>
      </div>
      <div className="h-1.5 overflow-hidden rounded-full bg-secondary">
        <div
          className="h-full rounded-full bg-[var(--accent-retro)]"
          style={{ width: `${pct}%` }}
        />
      </div>
    </div>
  );
}

export function MemberRaPresenceCard({ presence }: Props) {
  const artUrl = presence.imageBoxArtUrl ?? presence.imageIconUrl;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Now playing</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex gap-4">
          <MemberRaGameLink
            raGameId={presence.raGameId}
            isTracked={presence.isTracked}
            className="relative block h-24 w-24 shrink-0 overflow-hidden rounded-md bg-secondary/40 steam-bevel-inset"
          >
            {artUrl ? (
              <Image src={artUrl} alt="" fill className="object-cover" sizes="96px" />
            ) : (
              <div className="flex h-full items-center justify-center text-muted-foreground">
                <ImageOff className="size-5" />
              </div>
            )}
          </MemberRaGameLink>
          <div className="min-w-0 flex-1 space-y-1">
            <MemberRaGameLink
              raGameId={presence.raGameId}
              isTracked={presence.isTracked}
              className="block truncate text-lg font-medium text-[var(--accent-retro)] hover:underline"
            >
              {presence.title}
            </MemberRaGameLink>
            <ConsoleName
              name={presence.consoleName}
              iconUrl={presence.consoleIconUrl}
              fallback={`Console #${presence.consoleId}`}
            />
            {presence.richPresenceAt ? (
              <p className="text-xs text-muted-foreground">
                Updated <FormattedSyncTime value={presence.richPresenceAt} />
              </p>
            ) : null}
          </div>
        </div>
        {presence.richPresenceMsg ? (
          <p className="rounded-md border border-border bg-muted/30 px-3 py-2 text-sm text-foreground">
            {presence.richPresenceMsg}
          </p>
        ) : null}
        <AchievementProgress presence={presence} />
        {presence.unlockedAchievements.length > 0 ? (
          <div className="space-y-2">
            <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
              Unlocked in this game
            </p>
            <MemberRaUnlockedAchievementBadges achievements={presence.unlockedAchievements} />
          </div>
        ) : (
          <p className="text-xs text-muted-foreground">
            Achievement badges appear after the next score sync records game progress.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
