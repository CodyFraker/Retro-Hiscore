import type { MemberStandingDto } from "@/generated/api-client";
import { globalTopPercent } from "@/lib/format-global-rank";

export function filterTopLeaderboardStandings(
  standings: MemberStandingDto[],
  maxTopPercent = 20,
): MemberStandingDto[] {
  return standings
    .filter((standing) => {
      const topPercent = globalTopPercent(standing.globalRank, standing.globalEntryCount);
      return topPercent != null && topPercent <= maxTopPercent;
    })
    .sort((a, b) => {
      const aPercent = globalTopPercent(a.globalRank, a.globalEntryCount) ?? Number.POSITIVE_INFINITY;
      const bPercent = globalTopPercent(b.globalRank, b.globalEntryCount) ?? Number.POSITIVE_INFINITY;
      if (aPercent !== bPercent) {
        return aPercent - bPercent;
      }
      const aRank = a.globalRank ?? Number.POSITIVE_INFINITY;
      const bRank = b.globalRank ?? Number.POSITIVE_INFINITY;
      return aRank - bRank;
    });
}
