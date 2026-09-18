export function formatGlobalRank(
  rank: number | null | undefined,
  entryCount: number | null | undefined,
): string | null {
  if (rank == null) {
    return null;
  }

  if (entryCount != null && entryCount > 0) {
    const topPercent = formatGlobalPercentile(rank, entryCount);
    return `#${rank.toLocaleString()} of ${entryCount.toLocaleString()} · ${topPercent}`;
  }

  return `#${rank.toLocaleString()} globally`;
}

export function formatGlobalPercentile(
  rank: number | null | undefined,
  entryCount: number | null | undefined,
): string | null {
  if (rank == null || entryCount == null || entryCount <= 0) {
    return null;
  }

  const topPercent = (rank / entryCount) * 100;
  return `top ${topPercent.toFixed(1)}%`;
}

export function globalTopPercent(
  rank: number | null | undefined,
  entryCount: number | null | undefined,
): number | null {
  if (rank == null || entryCount == null || entryCount <= 0) {
    return null;
  }

  return (rank / entryCount) * 100;
}

export function isGlobalTopTier(
  rank: number | null | undefined,
  entryCount: number | null | undefined,
  maxTopPercent = 20,
): boolean {
  const topPercent = globalTopPercent(rank, entryCount);
  if (topPercent == null) {
    return false;
  }

  return topPercent <= maxTopPercent;
}
