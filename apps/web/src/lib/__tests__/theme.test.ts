import { DEFAULT_THEME, isThemeId, resolveTheme } from "../theme";

describe("theme", () => {
  describe("isThemeId", () => {
    it("accepts known theme ids", () => {
      // Arrange
      const steam = "steam";
      const aero = "frutiger-aero";

      // Act & Assert
      expect(isThemeId(steam)).toBe(true);
      expect(isThemeId(aero)).toBe(true);
    });

    it("rejects unknown values", () => {
      // Arrange
      const unknown = "neon-punk";

      // Act & Assert
      expect(isThemeId(unknown)).toBe(false);
    });
  });

  describe("resolveTheme", () => {
    it("returns default for null and invalid values", () => {
      // Act & Assert
      expect(resolveTheme(null)).toBe(DEFAULT_THEME);
      expect(resolveTheme(undefined)).toBe(DEFAULT_THEME);
      expect(resolveTheme("invalid")).toBe(DEFAULT_THEME);
    });

    it("returns the theme when valid", () => {
      // Act & Assert
      expect(resolveTheme("frutiger-aero")).toBe("frutiger-aero");
    });
  });
});
