import { ImageOff } from "lucide-react";
import Image from "next/image";
import type {
  GameAchievementCatalogItemDto,
  GameAchievementMemberDto,
  GameAchievementMemberUnlockDto,
} from "@/generated/api-client";

type Props = {
  achievements: GameAchievementCatalogItemDto[];
  members: GameAchievementMemberDto[];
  unlocks: GameAchievementMemberUnlockDto[];
};

export function GameAchievementGrid({ achievements, members, unlocks }: Props) {
  if (achievements.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        Achievement catalog is empty until someone syncs progress for this game.
      </p>
    );
  }

  if (members.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No friend achievement progress synced for this game yet.
      </p>
    );
  }

  const unlockSet = new Set(
    unlocks.map((u) => `${u.memberId}:${u.raAchievementId}`),
  );

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[32rem] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th className="sticky left-0 z-10 bg-background py-2 pr-4 font-medium">Achievement</th>
            {members.map((m) => (
              <th key={m.memberId} className="px-2 py-2 text-center font-medium">
                <span className="line-clamp-2 max-w-[4.5rem] text-xs">{m.displayName}</span>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {achievements.map((ach) => (
            <tr key={ach.raAchievementId} className="border-b border-border/60">
              <td className="sticky left-0 z-10 bg-background py-2 pr-4">
                <div className="flex items-center gap-2">
                  <div className="relative h-8 w-8 shrink-0 overflow-hidden rounded bg-secondary/40">
                    {ach.badgeUrl ? (
                      <Image src={ach.badgeUrl} alt="" fill className="object-contain p-0.5" sizes="32px" />
                    ) : (
                      <div className="flex h-full items-center justify-center text-muted-foreground">
                        <ImageOff className="size-3" />
                      </div>
                    )}
                  </div>
                  <div className="min-w-0">
                    <p className="truncate font-medium">{ach.title}</p>
                    <p className="font-mono text-xs text-muted-foreground">+{ach.points}</p>
                  </div>
                </div>
              </td>
              {members.map((m) => {
                const unlocked = unlockSet.has(`${m.memberId}:${ach.raAchievementId}`);
                return (
                  <td key={m.memberId} className="px-2 py-2 text-center">
                    <span
                      className={
                        unlocked
                          ? "inline-block size-3 rounded-full bg-[var(--accent-retro)]"
                          : "inline-block size-3 rounded-full bg-secondary"
                      }
                      title={unlocked ? "Unlocked" : "Locked"}
                    />
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
