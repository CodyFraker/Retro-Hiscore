import Link from "next/link";
import { ChampionshipStandingsPanel } from "@/components/dashboard/championship-standings-panel";
import { DashboardChaseCard } from "@/components/chase/dashboard-chase-card";
import { DashboardRecentAchievementsCard } from "@/components/dashboard/dashboard-recent-achievements-card";
import { GroupRecentGamesCard } from "@/components/dashboard/group-recent-games-card";
import { DashboardGroupActivityCard } from "@/components/dashboard/dashboard-group-activity-card";
import { GameOfTheWeekDashboardCard } from "@/components/game-of-the-week/game-of-the-week-dashboard-card";
import { PinnedGamesStrip } from "@/components/dashboard/pinned-games-strip";
import type {
  ChampionshipRowDto,
  DashboardAchievementActivityItemDto,
  DashboardGroupActivityResponse,
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
  MemberChaseItemDto,
  MemberPinDto,
  RecentGroupGameDto,
  SeasonStandingsResponse,
} from "@/generated/api-client";

type Props = {
  championship: ChampionshipRowDto[];
  memberCount: number;
  recentGroupGames: RecentGroupGameDto[];
  achievementActivity: DashboardAchievementActivityItemDto[];
  achievementsSyncedAt?: string | null;
  gameOfTheWeekPoll?: GameOfTheWeekCurrentPollDto | null;
  gameOfTheWeekLastWinner?: GameOfTheWeekHistoryItemDto | null;
  groupActivity?: DashboardGroupActivityResponse | null;
  isAdmin?: boolean;
  currentMemberId?: string | null;
  chaseItems?: MemberChaseItemDto[];
  pinnedGames?: MemberPinDto[];
  seasonStandings?: SeasonStandingsResponse;
};

export function DashboardActivityPrimary({
  championship,
  memberCount,
  recentGroupGames,
  achievementActivity,
  achievementsSyncedAt,
  gameOfTheWeekPoll,
  gameOfTheWeekLastWinner,
  groupActivity,
  isAdmin = false,
  currentMemberId,
  chaseItems = [],
  pinnedGames = [],
  seasonStandings,
}: Props) {
  return (
    <div className="space-y-8">
      <GameOfTheWeekDashboardCard
        poll={gameOfTheWeekPoll}
        lastWinner={gameOfTheWeekLastWinner}
        isAdmin={isAdmin}
        currentMemberId={currentMemberId}
      />
      {groupActivity ? <DashboardGroupActivityCard activity={groupActivity} /> : null}
      <DashboardChaseCard items={chaseItems} />
      {seasonStandings ? (
        <ChampionshipStandingsPanel
          championship={championship}
          memberCount={memberCount}
          initialSeason={seasonStandings}
          limit={5}
        />
      ) : null}
      <DashboardRecentAchievementsCard
        items={achievementActivity}
        achievementsSyncedAt={achievementsSyncedAt}
      />
      <GroupRecentGamesCard games={recentGroupGames} isAdmin={isAdmin} />
      <PinnedGamesStrip pins={pinnedGames} />
      <section className="rounded border border-dashed border-border p-6 text-center">
        <p className="text-sm text-muted-foreground">
          Search, sort, and compare every tracked game in the catalog.
        </p>
        <Link
          href="/games"
          className="mt-3 inline-block text-sm font-medium text-[var(--accent-retro)] hover:underline"
        >
          Browse all tracked games →
        </Link>
      </section>
    </div>
  );
}
