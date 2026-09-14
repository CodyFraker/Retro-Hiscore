import { formatFriendRank, formatStandingScore, formatSyncTimeUtc } from "../format";

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

describe("formatSyncTimeUtc", () => {
  it("returns Never when value is missing", () => {
    // Arrange / Act / Assert
    expect(formatSyncTimeUtc(null)).toBe("Never");
  });

  it("formats using UTC regardless of runtime timezone", () => {
    // Arrange
    const value = "2026-09-13T19:09:00.000Z";

    // Act
    const result = formatSyncTimeUtc(value);

    // Assert
    expect(result).toBe(
      new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
        timeZone: "UTC",
      }).format(new Date(value)),
    );
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
