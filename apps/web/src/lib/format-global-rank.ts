export function formatGlobalRank(
  rank: number | null | undefined,
  entryCount: number | null | undefined,
): string | null {
  if (rank == null) {
    return null;
  }

  if (entryCount != null && entryCount > 0) {
    return `#${rank.toLocaleString()} of ${entryCount.toLocaleString()}`;
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

  const percentile = ((entryCount - rank + 1) / entryCount) * 100;
  if (percentile >= 99) {
    return `top ${(100 - percentile + 1).toFixed(1)}%`;
  }

  return `top ${percentile.toFixed(1)}%`;
}
