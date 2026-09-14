import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { MemberAvatar } from "@/components/members/member-avatar";
import { RivalryGameSection } from "@/components/rivalry/rivalry-game-section";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ usernameA: string; usernameB: string }>;
};

function formatScore(score: number | null | undefined, formatted?: string | null) {
  if (formatted) {
    return formatted;
  }

  if (score == null) {
    return "—";
  }

  return score.toLocaleString();
}

function isCloseBattle(
  board: {
    memberAScore?: number | null;
    memberBScore?: number | null;
    memberAFriendRank?: number | null;
    memberBFriendRank?: number | null;
  },
) {
  if (
    board.memberAFriendRank != null &&
    board.memberBFriendRank != null &&
    Math.abs(board.memberAFriendRank - board.memberBFriendRank) === 1
  ) {
    return true;
  }

  if (board.memberAScore != null && board.memberBScore != null && board.memberAScore > 0) {
    const ratio = Math.min(board.memberAScore, board.memberBScore) / Math.max(board.memberAScore, board.memberBScore);
    return ratio >= 0.9;
  }

  return false;
}

export default async function RivalryPage({ params }: Props) {
  const { usernameA: rawA, usernameB: rawB } = await params;
  const usernameA = decodeURIComponent(rawA);
  const usernameB = decodeURIComponent(rawB);

  const api = await getServerApiClient();
  let rivalry: Awaited<ReturnType<typeof api.getRivalry>>;
  try {
    rivalry = await api.getRivalry(usernameA, usernameB);
  } catch {
    notFound();
  }

  const leader =
    rivalry.memberALeads > rivalry.memberBLeads
      ? rivalry.memberA
      : rivalry.memberBLeads > rivalry.memberALeads
        ? rivalry.memberB
        : null;

  const closestBattles = rivalry.games
    .flatMap((game) => game.boards.map((board) => ({ game, board })))
    .filter(({ board }) => isCloseBattle(board))
    .slice(0, 5);

  return (
    <div className="space-y-10">
      <div className="space-y-2">
        <Link
          href="/members"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4 shrink-0" />
          All members
        </Link>
        <h1 className="flex flex-wrap items-center gap-3 font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)] sm:text-3xl">
          <span className="inline-flex items-center gap-2">
            <MemberAvatar
              avatarUrl={rivalry.memberA.avatarUrl}
              displayName={rivalry.memberA.displayName}
              size={40}
            />
            {rivalry.memberA.displayName}
          </span>
          <span>vs</span>
          <span className="inline-flex items-center gap-2">
            <MemberAvatar
              avatarUrl={rivalry.memberB.avatarUrl}
              displayName={rivalry.memberB.displayName}
              size={40}
            />
            {rivalry.memberB.displayName}
          </span>
        </h1>
        <p className="text-sm text-muted-foreground">
          {leader
            ? `${leader.displayName} leads ${Math.max(rivalry.memberALeads, rivalry.memberBLeads)}–${Math.min(rivalry.memberALeads, rivalry.memberBLeads)}`
            : `Tied at ${rivalry.memberALeads}–${rivalry.memberBLeads}`}
          {rivalry.tiedBoards > 0 && (
            <>
              <span className="mx-2 text-border">·</span>
              {rivalry.tiedBoards} tied board{rivalry.tiedBoards === 1 ? "" : "s"}
            </>
          )}
        </p>
      </div>

      {closestBattles.length > 0 && (
        <section className="space-y-3">
          <h2 className="steam-section-heading">Closest battles</h2>
          <ul className="divide-y divide-border border-y border-border">
            {closestBattles.map(({ game, board }) => (
              <li
                key={board.raLeaderboardId}
                className="flex flex-col gap-1 py-3 text-sm sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <Link
                    href={`/games/${game.raGameId}`}
                    className="font-medium hover:text-[var(--accent-retro)]"
                  >
                    {game.gameTitle}
                  </Link>
                  <span className="text-muted-foreground"> · {board.title}</span>
                </div>
                <div className="font-mono text-xs text-muted-foreground">
                  {formatScore(board.memberAScore, board.memberAFormattedScore)} vs{" "}
                  {formatScore(board.memberBScore, board.memberBFormattedScore)}
                </div>
              </li>
            ))}
          </ul>
        </section>
      )}

      {rivalry.games.length === 0 ? (
        <p className="text-muted-foreground">No shared boards yet.</p>
      ) : (
        rivalry.games.map((game) => (
          <RivalryGameSection
            key={game.raGameId}
            game={game}
            memberA={rivalry.memberA}
            memberB={rivalry.memberB}
          />
        ))
      )}
    </div>
  );
}
