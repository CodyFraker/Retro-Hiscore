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

export function formatSyncTime(value: string | null | undefined) {
  if (!value) {
    return "Never";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
