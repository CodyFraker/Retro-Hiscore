import { formatFriendRank, formatStandingScore } from "../format";

describe("formatStandingScore", () => {
  it("returns em dash when score is missing", () => {
    // Arrange
    const standing = { score: null, formattedScore: null };

    // Act
    const result = formatStandingScore(standing);

    // Assert
    expect(result).toBe("—");
  });

  it("prefers formatted score when present", () => {
    // Arrange
    const standing = { score: 352750, formattedScore: "352,750" };

    // Act
    const result = formatStandingScore(standing);

    // Assert
    expect(result).toBe("352,750");
  });
});

describe("formatFriendRank", () => {
  it("returns em dash when rank is missing", () => {
    // Arrange / Act / Assert
    expect(formatFriendRank(null)).toBe("—");
  });

  it("prefixes rank with hash", () => {
    // Arrange / Act / Assert
    expect(formatFriendRank(2)).toBe("#2");
  });
});
