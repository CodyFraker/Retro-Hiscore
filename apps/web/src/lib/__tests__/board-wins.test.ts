import { recentlyUpdatedBoards, summarizeBoardWins } from "../board-wins";
import type { GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";

describe("summarizeBoardWins", () => {
  it("counts friend rank ones and boards with scores", () => {
    // Arrange
    const members: StandingMemberDto[] = [
      { id: "a", raUsername: "alice", displayName: "Alice" },
      { id: "b", raUsername: "bob", displayName: "Bob" },
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
            score: 10,
            friendRank: 1,
          },
          {
            memberId: "b",
            raUsername: "bob",
            displayName: "Bob",
            score: 5,
            friendRank: 2,
          },
        ],
      },
      {
        id: "2",
        raLeaderboardId: 2,
        title: "Board 2",
        rankAsc: false,
        standings: [
          {
            memberId: "a",
            raUsername: "alice",
            displayName: "Alice",
            score: null,
            friendRank: null,
          },
          {
            memberId: "b",
            raUsername: "bob",
            displayName: "Bob",
            score: 9,
            friendRank: 1,
          },
        ],
      },
    ];

    // Act
    const rows = summarizeBoardWins(leaderboards, members);

    // Assert
    expect(rows[0]).toMatchObject({ displayName: "Alice", friendRankOnes: 1, boardsWithScore: 1 });
    expect(rows[1]).toMatchObject({ displayName: "Bob", friendRankOnes: 1, boardsWithScore: 2 });
  });

  it("carries avatarUrl from members into summary rows", () => {
    // Arrange
    const members: StandingMemberDto[] = [
      {
        id: "a",
        raUsername: "alice",
        displayName: "Alice",
        avatarUrl: "https://cdn.discordapp.com/avatars/1/a.png",
      },
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
            avatarUrl: "https://cdn.discordapp.com/avatars/1/a.png",
            score: 10,
            friendRank: 1,
          },
        ],
      },
    ];

    // Act
    const rows = summarizeBoardWins(leaderboards, members);

    // Assert
    expect(rows[0].avatarUrl).toBe("https://cdn.discordapp.com/avatars/1/a.png");
  });
});

describe("recentlyUpdatedBoards", () => {
  it("returns boards ordered by latest scoreUpdatedAt", () => {
    // Arrange
    const leaderboards: GameLeaderboardDto[] = [
      {
        id: "1",
        raLeaderboardId: 1,
        title: "Old",
        rankAsc: false,
        standings: [
          {
            memberId: "a",
            raUsername: "alice",
            displayName: "Alice",
            scoreUpdatedAt: "2026-01-01T00:00:00Z",
          },
        ],
      },
      {
        id: "2",
        raLeaderboardId: 2,
        title: "New",
        rankAsc: false,
        standings: [
          {
            memberId: "a",
            raUsername: "alice",
            displayName: "Alice",
            scoreUpdatedAt: "2026-01-03T00:00:00Z",
          },
        ],
      },
    ];

    // Act
    const rows = recentlyUpdatedBoards(leaderboards, 1);

    // Assert
    expect(rows).toHaveLength(1);
    expect(rows[0].title).toBe("New");
  });
});
