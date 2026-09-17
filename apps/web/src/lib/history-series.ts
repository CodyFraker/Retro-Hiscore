import type { LeaderboardHistoryItemDto } from "@/generated/api-client";

export type HistoryPoint = {
  syncedAt: string;
  score: number;
  formattedScore: string;
  friendRank: number | null;
  globalRank: number | null;
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
  globalRankDelta: number | null;
  previousGlobalRank: number | null;
  currentGlobalRank: number | null;
};

export type EnrichedLeaderboardHistoryItem = LeaderboardHistoryItemDto & {
  globalRankDelta: number | null;
};

export type LeaderboardPopulationDelta = {
  currentCount: number;
  delta: number;
};

export type LeaderboardPopulationPoint = {
  syncedAt: string;
  entryCount: number;
};

export const LEADERBOARD_HISTORY_PAGE_SIZE = 10;

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
      formattedScore: item.formattedScore,
      friendRank: item.friendRank ?? null,
      globalRank: item.globalRank ?? null,
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
        globalRankDelta: null,
        previousGlobalRank: null,
        currentGlobalRank: last?.globalRank ?? null,
      };
    }

    const previous = member.points[member.points.length - 2];
    const current = member.points[member.points.length - 1];
    const friendRankDelta =
      previous.friendRank != null && current.friendRank != null
        ? previous.friendRank - current.friendRank
        : null;
    const globalRankDelta =
      previous.globalRank != null && current.globalRank != null
        ? previous.globalRank - current.globalRank
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
      globalRankDelta,
      previousGlobalRank: previous.globalRank,
      currentGlobalRank: current.globalRank,
    };
  });
}

export function toLeaderboardPopulationSeries(
  items: LeaderboardHistoryItemDto[],
): LeaderboardPopulationPoint[] {
  const bySync = new Map<string, number>();

  for (const item of items) {
    if (item.globalEntryCount != null && !bySync.has(item.syncedAt)) {
      bySync.set(item.syncedAt, item.globalEntryCount);
    }
  }

  return [...bySync.entries()]
    .map(([syncedAt, entryCount]) => ({ syncedAt, entryCount }))
    .sort((a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime());
}

export function computeLeaderboardPopulationDelta(
  items: LeaderboardHistoryItemDto[],
): LeaderboardPopulationDelta | null {
  const series = toLeaderboardPopulationSeries(items);
  if (series.length === 0) {
    return null;
  }

  const stamps = series.map((point) => point.syncedAt);
  const bySync = new Map(series.map((point) => [point.syncedAt, point.entryCount]));

  if (stamps.length === 0) {
    return null;
  }

  const currentCount = bySync.get(stamps[stamps.length - 1])!;
  if (stamps.length < 2) {
    return { currentCount, delta: 0 };
  }

  const previousCount = bySync.get(stamps[stamps.length - 2])!;
  return { currentCount, delta: currentCount - previousCount };
}

export function dedupeLeaderboardHistoryItems(
  items: LeaderboardHistoryItemDto[],
): LeaderboardHistoryItemDto[] {
  const byMember = new Map<string, LeaderboardHistoryItemDto[]>();

  for (const item of items) {
    const list = byMember.get(item.memberId) ?? [];
    list.push(item);
    byMember.set(item.memberId, list);
  }

  const kept: LeaderboardHistoryItemDto[] = [];

  for (const list of byMember.values()) {
    list.sort((a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime());

    for (let index = 0; index < list.length; index++) {
      const current = list[index];
      const previous = list[index - 1];
      if (!previous) {
        kept.push(current);
        continue;
      }

      const scoreUnchanged = current.score === previous.score;
      const globalRankUnchanged = current.globalRank === previous.globalRank;
      if (!scoreUnchanged || !globalRankUnchanged) {
        kept.push(current);
      }
    }
  }

  return kept.sort(
    (a, b) => new Date(b.syncedAt).getTime() - new Date(a.syncedAt).getTime(),
  );
}

export function parseLeaderboardHistoryPage(
  raw: string | undefined,
  pageCount: number,
): number {
  const parsed = Number(raw);
  if (!Number.isFinite(parsed) || parsed < 1) {
    return 1;
  }
  return Math.min(Math.floor(parsed), Math.max(1, pageCount));
}

export function enrichLeaderboardHistoryRows(
  items: LeaderboardHistoryItemDto[],
): EnrichedLeaderboardHistoryItem[] {
  const byMember = new Map<string, LeaderboardHistoryItemDto[]>();

  for (const item of items) {
    const list = byMember.get(item.memberId) ?? [];
    list.push(item);
    byMember.set(item.memberId, list);
  }

  for (const list of byMember.values()) {
    list.sort((a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime());
  }

  const deltaById = new Map<string, number | null>();
  for (const list of byMember.values()) {
    for (let index = 0; index < list.length; index++) {
      const current = list[index];
      const previous = list[index - 1];
      let globalRankDelta: number | null = null;
      if (previous && current.globalRank != null && previous.globalRank != null) {
        globalRankDelta = previous.globalRank - current.globalRank;
      }
      deltaById.set(current.id, globalRankDelta);
    }
  }

  return items.map((item) => ({
    ...item,
    globalRankDelta: deltaById.get(item.id) ?? null,
  }));
}

export function memberSeriesHasGlobalRankVariation(series: MemberSeries[]): boolean {
  for (const member of series) {
    const ranks = member.points
      .map((point) => point.globalRank)
      .filter((rank): rank is number => rank != null);
    if (new Set(ranks).size > 1) {
      return true;
    }
  }
  return false;
}
