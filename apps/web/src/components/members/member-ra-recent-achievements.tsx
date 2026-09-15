import { ImageOff } from "lucide-react";
import Image from "next/image";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { Badge } from "@/components/ui/badge";
import type { MemberRaRecentAchievementDto } from "@/generated/api-client";

type Props = {
  achievements: MemberRaRecentAchievementDto[];
  trackedOnly?: boolean;
  heading?: string;
};

export function MemberRaRecentAchievements({
  achievements,
  trackedOnly = false,
  heading = "Recent unlocks",
}: Props) {
  const visible = trackedOnly
    ? achievements.filter((a) => a.isTracked)
    : achievements;

  if (visible.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h3 className="steam-section-heading">{heading}</h3>
      <ul className="divide-y divide-border rounded-md border border-border">
        {visible.map((achievement) => (
          <li key={achievement.raAchievementId} className="flex gap-3 p-3">
            <div className="relative h-12 w-12 shrink-0 overflow-hidden rounded bg-secondary/40">
              {achievement.badgeUrl ? (
                <Image
                  src={achievement.badgeUrl}
                  alt=""
                  fill
                  className="object-contain p-0.5"
                  sizes="48px"
                />
              ) : (
                <div className="flex h-full items-center justify-center text-muted-foreground">
                  <ImageOff className="size-4" />
                </div>
              )}
            </div>
            <div className="min-w-0 flex-1 space-y-0.5">
              <div className="flex flex-wrap items-center gap-2">
                <p className="font-medium">{achievement.title}</p>
                <span className="font-mono text-xs text-muted-foreground">
                  +{achievement.points}
                </span>
                {achievement.hardcoreAchieved ? (
                  <Badge variant="outline" className="text-[10px] uppercase">
                    Hardcore
                  </Badge>
                ) : null}
              </div>
              <p className="text-sm text-muted-foreground">
                <MemberRaGameLink
                  raGameId={achievement.raGameId}
                  isTracked={achievement.isTracked}
                  className="hover:text-foreground hover:underline"
                >
                  {achievement.gameTitle}
                </MemberRaGameLink>
              </p>
              {achievement.dateAwarded ? (
                <p className="text-xs text-muted-foreground">
                  <FormattedSyncTime value={achievement.dateAwarded} />
                </p>
              ) : null}
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
