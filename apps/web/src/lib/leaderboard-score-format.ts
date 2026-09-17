const TIME_LEADERBOARD_FORMATS = new Set([
  "MILLISECS",
  "FRAMES",
  "TIME",
  "SECS",
  "MINUTES",
]);

function normalizeFormat(format: string | null | undefined): string {
  return (format ?? "").trim().toUpperCase();
}

export function isTimeLeaderboardFormat(format: string | null | undefined): boolean {
  return TIME_LEADERBOARD_FORMATS.has(normalizeFormat(format));
}

function formatMinutesSecondsCentiseconds(centiseconds: number): string {
  const cs = Math.max(0, Math.round(centiseconds));
  const totalSeconds = cs / 100;
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  if (hours > 0 || minutes >= 60) {
    const displayHours = hours + Math.floor(minutes / 60);
    const displayMinutes = minutes % 60;
    const secPart = seconds.toFixed(2).padStart(5, "0");
    return `${displayHours}h${String(displayMinutes).padStart(2, "0")}:${secPart}`;
  }

  const secPart = seconds.toFixed(2).padStart(5, "0");
  return `${minutes}:${secPart}`;
}

function formatMinutesSeconds(totalSeconds: number): string {
  const seconds = Math.max(0, Math.round(totalSeconds));
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hours > 0) {
    return `${hours}h${String(minutes).padStart(2, "0")}:${String(secs).padStart(2, "0")}`;
  }

  return `${minutes}:${String(secs).padStart(2, "0")}`;
}

function formatHoursMinutes(totalMinutes: number): string {
  const minutes = Math.max(0, Math.round(totalMinutes));
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h${String(mins).padStart(2, "0")}`;
}

export function formatLeaderboardScore(
  score: number,
  format: string | null | undefined,
): string {
  const normalized = normalizeFormat(format);

  switch (normalized) {
    case "MILLISECS":
      return formatMinutesSecondsCentiseconds(score);
    case "FRAMES":
    case "TIME":
      return formatMinutesSecondsCentiseconds((score * 100) / 60);
    case "SECS":
      return formatMinutesSeconds(score);
    case "MINUTES":
      return formatHoursMinutes(score);
    default:
      return score.toLocaleString();
  }
}

export function formatLeaderboardScoreDelta(
  delta: number,
  format: string | null | undefined,
): string {
  if (delta === 0) {
    return "0";
  }

  if (!isTimeLeaderboardFormat(format)) {
    const abs = Math.abs(delta).toLocaleString();
    if (delta > 0) return `+${abs}`;
    return `−${abs}`;
  }

  const sign = delta > 0 ? "+" : "−";
  return `${sign}${formatLeaderboardScore(Math.abs(delta), format)}`;
}
