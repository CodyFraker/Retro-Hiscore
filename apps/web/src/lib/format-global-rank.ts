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
