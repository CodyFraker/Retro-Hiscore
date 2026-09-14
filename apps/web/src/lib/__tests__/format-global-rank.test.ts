import { formatGlobalPercentile, formatGlobalRank } from "@/lib/format-global-rank";

describe("formatGlobalRank", () => {
  it("formats rank with entry count", () => {
    expect(formatGlobalRank(165, 10_247)).toBe("#165 of 10,247");
  });

  it("falls back when entry count is missing", () => {
    expect(formatGlobalRank(165, null)).toBe("#165 globally");
  });
});

describe("formatGlobalPercentile", () => {
  it("returns percentile when data is present", () => {
    expect(formatGlobalPercentile(1, 100)).toBe("top 1.0%");
    expect(formatGlobalPercentile(50, 100)).toBe("top 51.0%");
  });
});
