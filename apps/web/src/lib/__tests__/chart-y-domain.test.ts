import { chartYDomain, fromZeroYDomain, tightYDomain } from "../chart-y-domain";

describe("tightYDomain", () => {
  it("zooms to data range for large rank values", () => {
    const [min, max] = tightYDomain([100_000, 120_000]);

    expect(min).toBeGreaterThan(90_000);
    expect(max).toBeLessThan(130_000);
    expect(min).toBeLessThan(100_000);
    expect(max).toBeGreaterThan(120_000);
  });
});

describe("fromZeroYDomain", () => {
  it("never sets a negative lower bound", () => {
    const [min, max] = fromZeroYDomain([0, 53]);

    expect(min).toBe(0);
    expect(max).toBeGreaterThan(53);
  });
});

describe("chartYDomain", () => {
  it("uses fromZero mode for cumulative series", () => {
    const [min] = chartYDomain([5, 50], "fromZero");
    expect(min).toBe(0);
  });
});
