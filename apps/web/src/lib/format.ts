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
