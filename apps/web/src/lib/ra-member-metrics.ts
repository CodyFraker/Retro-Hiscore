export function formatRankLabel(rank?: number | null, totalRanked?: number | null) {
  if (rank == null) {
    return "—";
  }

  if (totalRanked == null || totalRanked <= 0) {
    return `#${rank.toLocaleString()}`;
  }

  const topPercent = (rank / totalRanked) * 100;
  return `#${rank.toLocaleString()} · top ${topPercent.toFixed(1)}%`;
}

export function formatRaTrend(rankDelta?: number | null, pointsDelta?: number | null) {
  if (rankDelta != null && rankDelta !== 0) {
    const arrow = rankDelta > 0 ? "↑" : "↓";
    return `${arrow}${Math.abs(rankDelta).toLocaleString()} rank`;
  }

  if (pointsDelta != null && pointsDelta !== 0) {
    const sign = pointsDelta > 0 ? "+" : "";
    return `${sign}${pointsDelta.toLocaleString()} pts`;
  }

  return null;
}

export function isOnlineOrPlaying(status?: string | null) {
  if (!status) {
    return false;
  }

  const normalized = status.toLowerCase();
  return normalized === "online" || normalized.includes("playing");
}

export function shouldShowCompactPresence(
  status?: string | null,
  gameTitle?: string | null,
) {
  return isOnlineOrPlaying(status) && Boolean(gameTitle?.trim());
}

export function retroAchievementsUserUrl(raUlid?: string | null, raUsername?: string | null) {
  const target = raUlid ?? raUsername;
  if (!target) {
    return null;
  }

  return `https://retroachievements.org/user/${encodeURIComponent(target)}`;
}
