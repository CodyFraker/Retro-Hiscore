import { summarizeBoardWins } from "../board-wins";
import type { GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";

describe("member avatar data", () => {
  it("preserves null avatarUrl when member has no Discord image", () => {
    // Arrange
    const members: StandingMemberDto[] = [
      { id: "a", raUsername: "alice", displayName: "Alice", avatarUrl: null },
    ];
    const leaderboards: GameLeaderboardDto[] = [
      {
        id: "1",
        raLeaderboardId: 1,
        title: "Board 1",
        rankAsc: false,
        standings: [
          {
            memberId: "a",
            raUsername: "alice",
            displayName: "Alice",
            avatarUrl: null,
            score: 10,
            friendRank: 1,
          },
        ],
      },
    ];

    // Act
    const rows = summarizeBoardWins(leaderboards, members);

    // Assert
    expect(rows[0].avatarUrl).toBeNull();
  });
});
