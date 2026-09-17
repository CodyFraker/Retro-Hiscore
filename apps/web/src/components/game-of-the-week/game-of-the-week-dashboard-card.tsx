import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryItemDto,
} from "@/generated/api-client";
import { GameOfTheWeekDashboardShowcase } from "@/components/game-of-the-week/game-of-the-week-dashboard-showcase";

type Props = {
  poll?: GameOfTheWeekCurrentPollDto | null;
  lastWinner?: GameOfTheWeekHistoryItemDto | null;
  isAdmin?: boolean;
  currentMemberId?: string | null;
};

export function GameOfTheWeekDashboardCard({
  poll,
  lastWinner,
  isAdmin,
  currentMemberId,
}: Props) {
  return (
    <GameOfTheWeekDashboardShowcase
      poll={poll}
      lastWinner={lastWinner}
      isAdmin={isAdmin}
      currentMemberId={currentMemberId}
    />
  );
}
