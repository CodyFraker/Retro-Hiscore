import type { FriendStandingDto } from "@/generated/api-client";


export function formatStandingScore(standing: Pick<FriendStandingDto, "formattedScore" | "score"> | null | undefined) {
  if (!standing || standing.score == null) {
    return "—";
  }

  return standing.formattedScore || String(standing.score);
}

export function formatFriendRank(rank: number | null | undefined) {
  if (rank == null) {
    return "—";
  }

  return `#${rank}`;
}

const syncTimeFormatOptions: Intl.DateTimeFormatOptions = {
  dateStyle: "medium",
  timeStyle: "short",
};

export function formatSyncTimeUtc(value: string | null | undefined) {
  if (!value) {
    return "Never";
  }

  return new Intl.DateTimeFormat(undefined, {
    ...syncTimeFormatOptions,
    timeZone: "UTC",
  }).format(new Date(value));
}

export function formatSyncTime(value: string | null | undefined) {
  if (!value) {
    return "Never";
  }

  return new Intl.DateTimeFormat(undefined, syncTimeFormatOptions).format(new Date(value));
}

export function formatRelativeTime(value: string | null | undefined) {
  if (!value) {
    return "—";
  }

  const diffSec = Math.round((new Date(value).getTime() - Date.now()) / 1000);
  const rtf = new Intl.RelativeTimeFormat(undefined, { numeric: "auto" });

  const absSec = Math.abs(diffSec);
  if (absSec < 60) {
    return rtf.format(diffSec, "second");
  }

  const diffMin = Math.round(diffSec / 60);
  if (Math.abs(diffMin) < 60) {
    return rtf.format(diffMin, "minute");
  }

  const diffHour = Math.round(diffSec / 3600);
  if (Math.abs(diffHour) < 24) {
    return rtf.format(diffHour, "hour");
  }

  const diffDay = Math.round(diffSec / 86400);
  if (Math.abs(diffDay) < 30) {
    return rtf.format(diffDay, "day");
  }

  const diffMonth = Math.round(diffSec / (86400 * 30));
  if (Math.abs(diffMonth) < 12) {
    return rtf.format(diffMonth, "month");
  }

  const diffYear = Math.round(diffSec / (86400 * 365));
  return rtf.format(diffYear, "year");
}
