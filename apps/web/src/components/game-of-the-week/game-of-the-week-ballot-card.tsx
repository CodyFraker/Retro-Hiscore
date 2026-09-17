import Image from "next/image";
import type { GameOfTheWeekBallotItemDto } from "@/generated/api-client";
import { GameOfTheWeekBallotTitle } from "@/components/game-of-the-week/game-of-the-week-ballot-title";
import { GameOfTheWeekVoterList } from "@/components/game-of-the-week/game-of-the-week-voter-list";
import { gameOfTheWeekImageUrl } from "@/lib/game-of-the-week-media";

type Props = {
  item: GameOfTheWeekBallotItemDto;
  mode: "readOnly" | "voting";
  selectedVote?: number | null;
  onSelectVote?: (raGameId: number) => void;
  votePending?: boolean;
  currentMemberId?: string | null;
  compactVoters?: boolean;
};

export function GameOfTheWeekBallotCard({
  item,
  mode,
  selectedVote,
  onSelectVote,
  votePending = false,
  currentMemberId,
  compactVoters = false,
}: Props) {
  const imageUrl = gameOfTheWeekImageUrl(item.imageIcon);

  return (
    <div className="rounded border border-border p-4">
      <div className="flex gap-3">
        {imageUrl ? (
          <Image
            src={imageUrl}
            alt=""
            width={48}
            height={48}
            className="rounded"
            unoptimized
          />
        ) : null}
        <div className="min-w-0 flex-1">
          <GameOfTheWeekBallotTitle raGameId={item.raGameId} title={item.title} isTracked={item.isTracked} />
          <p className="text-xs text-muted-foreground">{item.consoleName ?? "Unknown platform"}</p>
          {item.addedByDisplayName ? (
            <p className="mt-0.5 text-xs text-muted-foreground">Nominated by {item.addedByDisplayName}</p>
          ) : null}
          <p className="mt-1 text-xs text-muted-foreground">
            {item.voteCount} vote{item.voteCount === 1 ? "" : "s"}
            {item.isTracked ? " · On site" : " · Not tracked yet"}
          </p>
        </div>
      </div>
      <GameOfTheWeekVoterList
        voters={item.voters}
        currentMemberId={currentMemberId}
        compact={compactVoters}
      />
      {mode === "voting" ? (
        <label className="mt-3 flex cursor-pointer items-center gap-2 text-sm">
          <input
            type="radio"
            name="gotw-vote"
            checked={selectedVote === item.raGameId}
            disabled={votePending}
            onChange={() => onSelectVote?.(item.raGameId)}
          />
          My vote
        </label>
      ) : null}
    </div>
  );
}
