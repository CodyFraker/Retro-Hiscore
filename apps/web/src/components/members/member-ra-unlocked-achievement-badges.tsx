import { ImageOff } from "lucide-react";
import Image from "next/image";
import type { MemberRaUnlockedAchievementDto } from "@/generated/api-client";

type Props = {
  achievements: MemberRaUnlockedAchievementDto[];
  size?: "sm" | "md";
  maxVisible?: number;
};

function AchievementBadge({
  achievement,
  size,
}: {
  achievement: MemberRaUnlockedAchievementDto;
  size: "sm" | "md";
}) {
  const dim = size === "sm" ? "h-8 w-8" : "h-12 w-12";
  const iconSize = size === "sm" ? "size-3" : "size-4";
  const label = `${achievement.title} (+${achievement.points})`;

  return (
    <div
      className={`relative shrink-0 overflow-hidden rounded bg-secondary/40 ${dim}`}
      title={label}
    >
      {achievement.badgeUrl ? (
        <Image
          src={achievement.badgeUrl}
          alt=""
          fill
          className="object-contain p-0.5"
          sizes={size === "sm" ? "32px" : "48px"}
        />
      ) : (
        <div className="flex h-full items-center justify-center text-muted-foreground">
          <ImageOff className={iconSize} />
        </div>
      )}
    </div>
  );
}

export function MemberRaUnlockedAchievementBadges({
  achievements,
  size = "md",
  maxVisible,
}: Props) {
  if (achievements.length === 0) {
    return null;
  }

  const visible = maxVisible != null ? achievements.slice(0, maxVisible) : achievements;
  const overflow = maxVisible != null ? achievements.length - visible.length : 0;

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {visible.map((achievement) => (
        <AchievementBadge key={achievement.raAchievementId} achievement={achievement} size={size} />
      ))}
      {overflow > 0 ? (
        <span className="text-xs text-muted-foreground">+{overflow}</span>
      ) : null}
    </div>
  );
}
