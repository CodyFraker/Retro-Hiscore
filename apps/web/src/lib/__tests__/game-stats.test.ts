import type { GameLeaderboardsResponse } from "@/generated/api-client";
import { summarizeGameStats } from "../game-stats";

function gameData(overrides: Partial<GameLeaderboardsResponse> = {}): GameLeaderboardsResponse {
  return {
    gameId: "11111111-1111-1111-1111-111111111111",
    raGameId: 100,
    title: "Test Game",
    members: [
      { id: "m1", raUsername: "alice", displayName: "Alice" },
      { id: "m2", raUsername: "bob", displayName: "Bob" },
    ],
    leaderboards: [
      {
        id: "b1",
        raLeaderboardId: 1001,
        title: "Board A",
        rankAsc: false,
        standings: [
          {
            memberId: "m1",
            raUsername: "alice",
            displayName: "Alice",
            score: 100,
            formattedScore: "100",
            friendRank: 1,
            scoreUpdatedAt: "2026-01-02T00:00:00Z",
          },
          {
            memberId: "m2",
            raUsername: "bob",
            displayName: "Bob",
            score: 50,
            formattedScore: "50",
            friendRank: 2,
            scoreUpdatedAt: "2026-01-01T00:00:00Z",
          },
        ],
      },
      {
        id: "b2",
        raLeaderboardId: 1002,
        title: "Board B",
        rankAsc: false,
        standings: [
          {
            memberId: "m1",
            raUsername: "alice",
            displayName: "Alice",
            score: null,
            formattedScore: null,
            friendRank: null,
          },
          {
            memberId: "m2",
            raUsername: "bob",
            displayName: "Bob",
            score: 200,
            formattedScore: "200",
            friendRank: 1,
            scoreUpdatedAt: "2026-01-03T00:00:00Z",
          },
        ],
      },
    ],
    ...overrides,
  };
}

describe("summarizeGameStats", () => {
  it("counts leaderboards, scored members, and total scores", () => {
    // Arrange
    const data = gameData();

    // Act
    const stats = summarizeGameStats(data);

    // Assert
    expect(stats.leaderboardCount).toBe(2);
    expect(stats.membersWithScores).toBe(2);
    expect(stats.totalScores).toBe(3);
    expect(stats.lastActivityAt).toBe("2026-01-03T00:00:00Z");
  });

  it("returns the member with the most friend rank ones as current leader", () => {
    // Arrange
    const data = gameData();

    // Act
    const stats = summarizeGameStats(data);

    // Assert
    expect(stats.currentLeader).toEqual({
      memberId: "m1",
      displayName: "Alice",
      friendRankOnes: 1,
    });
  });

  it("returns null current leader when no one leads a board", () => {
    // Arrange
    const data = gameData({
      leaderboards: [
        {
          id: "b1",
          raLeaderboardId: 1001,
          title: "Board A",
          rankAsc: false,
          standings: [
            {
              memberId: "m1",
              raUsername: "alice",
              displayName: "Alice",
              score: null,
              formattedScore: null,
              friendRank: null,
            },
          ],
        },
      ],
      members: [{ id: "m1", raUsername: "alice", displayName: "Alice" }],
    });

    // Act
    const stats = summarizeGameStats(data);

    // Assert
    expect(stats.currentLeader).toBeNull();
    expect(stats.totalScores).toBe(0);
  });
});
