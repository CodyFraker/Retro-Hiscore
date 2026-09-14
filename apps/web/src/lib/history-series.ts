import type { LeaderboardHistoryItemDto } from "@/generated/api-client";

export type HistoryPoint = {
  syncedAt: string;
  score: number;
  friendRank: number | null;
};

export type MemberSeries = {
  memberId: string;
  displayName: string;
  avatarUrl?: string | null;
  points: HistoryPoint[];
};

export type MemberDelta = {
  memberId: string;
  displayName: string;
  scoreDelta: number | null;
  previousScore: number | null;
  currentScore: number | null;
  friendRankDelta: number | null;
  previousFriendRank: number | null;
  currentFriendRank: number | null;
};

export function toChartSeries(items: LeaderboardHistoryItemDto[]): MemberSeries[] {
  const byMember = new Map<string, MemberSeries>();

  for (const item of items) {
    let series = byMember.get(item.memberId);
    if (!series) {
      series = {
        memberId: item.memberId,
        displayName: item.displayName,
        avatarUrl: item.avatarUrl,
        points: [],
      };
      byMember.set(item.memberId, series);
    }

    series.points.push({
      syncedAt: item.syncedAt,
      score: item.score,
      friendRank: item.friendRank ?? null,
    });
  }

  return [...byMember.values()]
    .map((series) => ({
      ...series,
      points: [...series.points].sort(
        (a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime(),
      ),
    }))
    .sort((a, b) => a.displayName.localeCompare(b.displayName));
}

export function countDistinctSyncTimestamps(series: MemberSeries[]): number {
  const stamps = new Set<string>();
  for (const member of series) {
    for (const point of member.points) {
      stamps.add(point.syncedAt);
    }
  }
  return stamps.size;
}

export function computeMemberDeltas(series: MemberSeries[]): MemberDelta[] {
  return series.map((member) => {
    if (member.points.length < 2) {
      const last = member.points.at(-1);
      return {
        memberId: member.memberId,
        displayName: member.displayName,
        scoreDelta: null,
        previousScore: null,
        currentScore: last?.score ?? null,
        friendRankDelta: null,
        previousFriendRank: null,
        currentFriendRank: last?.friendRank ?? null,
      };
    }

    const previous = member.points[member.points.length - 2];
    const current = member.points[member.points.length - 1];
    const friendRankDelta =
      previous.friendRank != null && current.friendRank != null
        ? previous.friendRank - current.friendRank
        : null;

    return {
      memberId: member.memberId,
      displayName: member.displayName,
      scoreDelta: current.score - previous.score,
      previousScore: previous.score,
      currentScore: current.score,
      friendRankDelta,
      previousFriendRank: previous.friendRank,
      currentFriendRank: current.friendRank,
    };
  });
}
