import { PageHero } from "@/components/layout/page-hero";
import { GameOfTheWeekIdlePanel } from "@/components/game-of-the-week/game-of-the-week-idle-panel";
import { GameOfTheWeekView } from "@/components/game-of-the-week/game-of-the-week-view";
import type {
  GameOfTheWeekCurrentPollDto,
  GameOfTheWeekHistoryResponse,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

async function loadHistory(): Promise<GameOfTheWeekHistoryResponse> {
  try {
    const api = await getServerApiClient();
    return await api.getGameOfTheWeekHistory(20, 0);
  } catch {
    return { total: 0, offset: 0, limit: 20, items: [] };
  }
}

export default async function GameOfTheWeekPage() {
  const history = await loadHistory();
  let currentPoll: GameOfTheWeekCurrentPollDto | null = null;

  try {
    const api = await getServerApiClient();
    currentPoll = await api.getGameOfTheWeekCurrent();
  } catch {
    currentPoll = null;
  }

  if (currentPoll) {
    return (
      <div className="space-y-8">
        <PageHero title="Game of the week" />
        <GameOfTheWeekView initialPoll={currentPoll} history={history.items} />
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <PageHero title="Game of the week" />
      <GameOfTheWeekIdlePanel history={history.items} />
    </div>
  );
}
