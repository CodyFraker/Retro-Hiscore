import type { GameLeaderboardSyncStatusDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";

export function leaderboardSyncTierLabel(
  tier: string,
  forcedCold = false,
): string {
  if (forcedCold) {
    return "Cold sync (pinned)";
  }

  return tier === "Hot" ? "Hot sync" : "Cold sync";
}

export function leaderboardSyncTierDescription(
  status: GameLeaderboardSyncStatusDto,
  formatTime: (value: string | null | undefined) => string = formatSyncTime,
): string {
  const interval = status.leaderboardSyncIntervalMinutes;
  const intervalLabel =
    interval >= 60 * 24
      ? `about every ${Math.round(interval / (60 * 24))} day(s)`
      : interval >= 60
        ? `about every ${Math.round(interval / 60)} hour(s)`
        : `about every ${interval} minute(s)`;

  const pinnedPart = status.leaderboardSyncForcedCold
    ? "An admin pinned this game to the cold schedule to limit RetroAchievements API usage. "
    : "";

  const playedPart = status.leaderboardSyncForcedCold
    ? ""
    : status.groupLastPlayedAt
      ? `Someone in the group played this game ${formatTime(status.groupLastPlayedAt)}. `
      : "No recent group play on record. ";

  const duePart = status.leaderboardSyncIsDue
    ? "Leaderboard sync is due now or pending."
    : status.leaderboardSyncNextDueAt
      ? `Next leaderboard sync around ${formatTime(status.leaderboardSyncNextDueAt)}.`
      : "";

  return `${pinnedPart}${playedPart}${status.tier === "Hot" ? "Hot" : "Cold"} schedule: scores refresh ${intervalLabel}. ${duePart}`.trim();
}
