import { parseRaGameTitle } from "../ra-game-title";

describe("parseRaGameTitle", () => {
  it("returns the title unchanged when no mod markers are present", () => {
    const result = parseRaGameTitle("Sonic the Hedgehog");

    expect(result.displayTitle).toBe("Sonic the Hedgehog");
    expect(result.modTags).toEqual([]);
  });

  it("strips ~Homebrew~ and adds a homebrew tag", () => {
    const result = parseRaGameTitle("My Game ~Homebrew~");

    expect(result.displayTitle).toBe("My Game");
    expect(result.modTags).toEqual(["homebrew"]);
  });

  it("strips ~Hack~ and adds a hack tag", () => {
    const result = parseRaGameTitle("~Hack~ Super Mario Bros.");

    expect(result.displayTitle).toBe("Super Mario Bros.");
    expect(result.modTags).toEqual(["hack"]);
  });

  it("detects both markers case-insensitively and lists both tags", () => {
    const result = parseRaGameTitle("Quest ~hack~ Edition ~HOMEBREW~");

    expect(result.displayTitle).toBe("Quest Edition");
    expect(result.modTags).toEqual(["homebrew", "hack"]);
  });
});
