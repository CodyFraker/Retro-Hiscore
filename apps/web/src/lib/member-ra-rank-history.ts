import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";

export type MemberRaRankSnapshotRow = MemberRaRankHistoryItemDto & {
  rankDelta: number | null;
  hardcorePointsDelta: number | null;
};

export type MemberRaRankHistorySummary = {
  currentRank: number | null;
  currentTotalRanked: number | null;
  currentTotalPoints: number | null;
  currentTotalTruePoints: number | null;
  currentTotalSoftcorePoints: number | null;
  bestRank: number | null;
  periodRankDelta: number | null;
  periodHardcorePointsDelta: number | null;
  periodStartSyncedAt: string | null;
  latestHardcorePointsDelta: number | null;
};

function bySyncedAtAsc(a: MemberRaRankHistoryItemDto, b: MemberRaRankHistoryItemDto) {
  return new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime();
}

function bySyncedAtDesc(a: MemberRaRankHistoryItemDto, b: MemberRaRankHistoryItemDto) {
  return new Date(b.syncedAt).getTime() - new Date(a.syncedAt).getTime();
}

export function buildRankSnapshotRows(items: MemberRaRankHistoryItemDto[]): MemberRaRankSnapshotRow[] {
  const newestFirst = [...items].sort(bySyncedAtDesc);

  return newestFirst.map((item, index) => {
    const older = newestFirst[index + 1];
    let rankDelta: number | null = null;
    if (item.rank != null && older?.rank != null) {
      rankDelta = older.rank - item.rank;
    }

    let hardcorePointsDelta: number | null = null;
    if (item.totalPoints != null && older?.totalPoints != null) {
      hardcorePointsDelta = item.totalPoints - older.totalPoints;
    }

    return { ...item, rankDelta, hardcorePointsDelta };
  });
}

export function summarizeRankHistory(items: MemberRaRankHistoryItemDto[]): MemberRaRankHistorySummary {
  if (items.length === 0) {
    return {
      currentRank: null,
      currentTotalRanked: null,
      currentTotalPoints: null,
      currentTotalTruePoints: null,
      currentTotalSoftcorePoints: null,
      bestRank: null,
      periodRankDelta: null,
      periodHardcorePointsDelta: null,
      periodStartSyncedAt: null,
      latestHardcorePointsDelta: null,
    };
  }

  const chronological = [...items].sort(bySyncedAtAsc);
  const first = chronological[0];
  const last = chronological[chronological.length - 1];

  const ranks = chronological.map((item) => item.rank).filter((rank): rank is number => rank != null);
  const bestRank = ranks.length > 0 ? Math.min(...ranks) : null;

  let periodRankDelta: number | null = null;
  if (first.rank != null && last.rank != null && chronological.length > 1) {
    periodRankDelta = first.rank - last.rank;
  }

  let latestHardcorePointsDelta: number | null = null;
  if (chronological.length > 1) {
    const previous = chronological[chronological.length - 2];
    if (last.totalPoints != null && previous.totalPoints != null) {
      latestHardcorePointsDelta = last.totalPoints - previous.totalPoints;
    }
  }

  let periodHardcorePointsDelta: number | null = null;
  if (first.totalPoints != null && last.totalPoints != null && chronological.length > 1) {
    periodHardcorePointsDelta = last.totalPoints - first.totalPoints;
  }

  return {
    currentRank: last.rank ?? null,
    currentTotalRanked: last.totalRanked ?? null,
    currentTotalPoints: last.totalPoints ?? null,
    currentTotalTruePoints: last.totalTruePoints ?? null,
    currentTotalSoftcorePoints: last.totalSoftcorePoints ?? null,
    bestRank,
    periodRankDelta,
    periodHardcorePointsDelta,
    periodStartSyncedAt: chronological.length > 1 ? first.syncedAt : null,
    latestHardcorePointsDelta,
  };
}

export function softcorePointsVaryInHistory(items: MemberRaRankHistoryItemDto[]) {
  const values = items
    .map((item) => item.totalSoftcorePoints)
    .filter((value): value is number => value != null);
  if (values.length < 2) {
    return false;
  }
  const min = Math.min(...values);
  const max = Math.max(...values);
  return min !== max;
}

export function formatRankDelta(delta: number | null | undefined) {
  if (delta == null || delta === 0) {
    return "—";
  }

  const arrow = delta > 0 ? "↑" : "↓";
  return `${arrow}${Math.abs(delta).toLocaleString()}`;
}

export function formatPointsDelta(delta: number | null | undefined) {
  if (delta == null || delta === 0) {
    return "—";
  }

  const sign = delta > 0 ? "+" : "";
  return `${sign}${delta.toLocaleString()}`;
}
