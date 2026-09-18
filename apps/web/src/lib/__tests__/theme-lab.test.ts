import {
  exportThemeLabCss,
  FRUTIGER_AERO_DEFAULT_GRADIENTS,
  FRUTIGER_AERO_DEFAULT_PALETTE,
  getDefaultThemeLab,
  hslStringToHex,
  isThemeLabEditable,
  normalizeHexColor,
  normalizeOpacityPercent,
} from "../theme-lab";

describe("theme-lab", () => {
  describe("isThemeLabEditable", () => {
    it("locks steam and allows frutiger-aero", () => {
      // Act & Assert
      expect(isThemeLabEditable("steam")).toBe(false);
      expect(isThemeLabEditable("frutiger-aero")).toBe(true);
    });
  });

  describe("exportThemeLabCss", () => {
    it("formats a data-theme block with palette and gradient variables", () => {
      // Arrange
      const lab = getDefaultThemeLab("frutiger-aero");

      // Act
      const css = exportThemeLabCss("frutiger-aero", lab!);

      // Assert
      expect(css).toContain('[data-theme="frutiger-aero"]');
      expect(css).toContain("--aero-cerulean: #5E9C9A;");
      expect(css).toContain("--aero-bg-gradient-start: #2B3D50;");
      expect(css).toContain("--muted-foreground: #C5E8F5;");
    });
  });

  describe("normalizeOpacityPercent", () => {
    it("accepts values from 0 to 100", () => {
      // Act & Assert
      expect(normalizeOpacityPercent("55")).toBe("55");
      expect(normalizeOpacityPercent("55%")).toBe("55");
      expect(normalizeOpacityPercent("101")).toBeNull();
    });
  });

  describe("normalizeHexColor", () => {
    it("adds a hash and uppercases valid six-digit hex", () => {
      // Act & Assert
      expect(normalizeHexColor("0092d6")).toBe("#0092D6");
      expect(normalizeHexColor("#0092d6")).toBe("#0092D6");
    });

    it("returns null for invalid values", () => {
      // Act & Assert
      expect(normalizeHexColor("")).toBeNull();
      expect(normalizeHexColor("0092")).toBeNull();
      expect(normalizeHexColor("not-a-color")).toBeNull();
    });
  });

  describe("hslStringToHex", () => {
    it("converts hsl strings to hex", () => {
      // Act
      const hex = hslStringToHex("hsl(0, 100%, 50%)");

      // Assert
      expect(hex).toBe("#FF0000");
    });
  });
});
