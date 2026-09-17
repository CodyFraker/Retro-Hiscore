import {
  formatLeaderboardScore,
  formatLeaderboardScoreDelta,
  isTimeLeaderboardFormat,
} from "../leaderboard-score-format";

describe("isTimeLeaderboardFormat", () => {
  it("recognizes RA time format identifiers", () => {
    expect(isTimeLeaderboardFormat("MILLISECS")).toBe(true);
    expect(isTimeLeaderboardFormat("FRAMES")).toBe(true);
    expect(isTimeLeaderboardFormat("VALUE")).toBe(false);
  });
});

describe("formatLeaderboardScore", () => {
  it("formats centisecond scores as minutes and seconds", () => {
    expect(formatLeaderboardScore(12620, "MILLISECS")).toBe("2:06.20");
    expect(formatLeaderboardScore(11740, "MILLISECS")).toBe("1:57.40");
  });

  it("formats numeric value leaderboards with grouping", () => {
    expect(formatLeaderboardScore(352750, "VALUE")).toBe("352,750");
  });
});

describe("formatLeaderboardScoreDelta", () => {
  it("formats time deltas using the leaderboard format", () => {
    expect(formatLeaderboardScoreDelta(-100, "MILLISECS")).toBe("−0:01.00");
    expect(formatLeaderboardScoreDelta(50, "VALUE")).toBe("+50");
  });
});
