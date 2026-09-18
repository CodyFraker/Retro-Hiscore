import type { ThemeId } from "@/lib/theme";

export const THEME_LAB_STORAGE_KEY = "retro-hiscore-theme-lab-palette";

export type PaletteCssVar =
  | "--aero-cerulean"
  | "--aero-persian-green"
  | "--aero-robin-egg"
  | "--aero-cream-can"
  | "--aero-flamingo";

export type SemanticCssVar =
  | "--foreground"
  | "--muted-foreground"
  | "--card-foreground"
  | "--primary-foreground"
  | "--secondary-foreground"
  | "--accent-foreground"
  | "--popover-foreground"
  | "--background";

export type GradientCssVar =
  | "--aero-bg-glow-top"
  | "--aero-bg-glow-right"
  | "--aero-bg-glow-bottom"
  | "--aero-bg-glow-top-opacity"
  | "--aero-bg-glow-right-opacity"
  | "--aero-bg-glow-bottom-opacity"
  | "--aero-bg-gradient-start"
  | "--aero-bg-gradient-mid"
  | "--aero-bg-gradient-end";

export type ThemePalette = Record<PaletteCssVar, string>;

export type ThemeGradients = Record<GradientCssVar, string>;

export type ThemeSemantics = Record<SemanticCssVar, string>;

export type ThemeLabOverrides = {
  palette: ThemePalette;
  gradients: ThemeGradients;
  semantics: ThemeSemantics;
};

export type PaletteFieldMeta = {
  name: string;
  usage: string;
};

export type GradientFieldMeta = {
  name: string;
  usage: string;
  kind: "color" | "opacity";
};

export const FRUTIGER_AERO_DEFAULT_PALETTE: ThemePalette = {
  "--aero-cerulean": "#5E9C9A",
  "--aero-persian-green": "#A7D7D3",
  "--aero-robin-egg": "#C8C3D5",
  "--aero-cream-can": "#FAB700",
  "--aero-flamingo": "#F5683D",
};

export const FRUTIGER_AERO_DEFAULT_SEMANTICS: ThemeSemantics = {
  "--foreground": "#F0F9FF",
  "--muted-foreground": "#C5E8F5",
  "--card-foreground": "#0C2D42",
  "--primary-foreground": "#FFFFFF",
  "--secondary-foreground": "#0C2D42",
  "--accent-foreground": "#0C2D42",
  "--popover-foreground": "#0C2D42",
  "--background": "#0A6EA8",
};

export const FRUTIGER_AERO_DEFAULT_GRADIENTS: ThemeGradients = {
  "--aero-bg-glow-top": "#003D7A",
  "--aero-bg-glow-right": "#004F9E",
  "--aero-bg-glow-bottom": "#003C8A",
  "--aero-bg-glow-top-opacity": "60",
  "--aero-bg-glow-right-opacity": "70",
  "--aero-bg-glow-bottom-opacity": "70",
  "--aero-bg-gradient-start": "#2B3D50",
  "--aero-bg-gradient-mid": "#046762",
  "--aero-bg-gradient-end": "#0092D6",
};

export const THEME_PALETTE_CONFIG: Partial<
  Record<
    ThemeId,
    {
      locked: boolean;
      defaults: ThemePalette;
      gradientDefaults: ThemeGradients;
      semanticDefaults: ThemeSemantics;
      fields: Record<PaletteCssVar, PaletteFieldMeta>;
      gradientFields: Record<GradientCssVar, GradientFieldMeta>;
      semanticFields: Record<SemanticCssVar, PaletteFieldMeta>;
    }
  >
> = {
  "frutiger-aero": {
    locked: false,
    defaults: FRUTIGER_AERO_DEFAULT_PALETTE,
    gradientDefaults: FRUTIGER_AERO_DEFAULT_GRADIENTS,
    semanticDefaults: FRUTIGER_AERO_DEFAULT_SEMANTICS,
    fields: {
      "--aero-cerulean": {
        name: "Cerulean",
        usage:
          "Primary buttons, card titles, and chart series 1; tints glass cards and the sidebar base.",
      },
      "--aero-persian-green": {
        name: "Persian green",
        usage: "Secondary buttons and panels, sidebar accent areas, and chart series 2.",
      },
      "--aero-robin-egg": {
        name: "Robin's egg",
        usage:
          "Accent highlights, focus rings, muted fills, popover tint, sidebar ring, and chart series 3.",
      },
      "--aero-cream-can": {
        name: "Cream can",
        usage:
          "Link and logo color (accent-retro), active nav emphasis, sidebar primary, and chart series 4.",
      },
      "--aero-flamingo": {
        name: "Flamingo",
        usage: "Destructive actions, error states, and chart series 5.",
      },
    },
    gradientFields: {
      "--aero-bg-glow-top": {
        name: "Top aurora glow",
        usage: "Upper-left radial highlight on the page background (Robin’s egg by default).",
        kind: "color",
      },
      "--aero-bg-glow-right": {
        name: "Right aurora glow",
        usage: "Upper-right radial highlight on the page background.",
        kind: "color",
      },
      "--aero-bg-glow-bottom": {
        name: "Bottom aurora glow",
        usage: "Lower radial highlight behind content on the page background.",
        kind: "color",
      },
      "--aero-bg-glow-top-opacity": {
        name: "Top glow strength",
        usage: "Opacity of the upper-left radial (0–100).",
        kind: "opacity",
      },
      "--aero-bg-glow-right-opacity": {
        name: "Right glow strength",
        usage: "Opacity of the upper-right radial (0–100).",
        kind: "opacity",
      },
      "--aero-bg-glow-bottom-opacity": {
        name: "Bottom glow strength",
        usage: "Opacity of the lower radial (0–100).",
        kind: "opacity",
      },
      "--aero-bg-gradient-start": {
        name: "Base gradient start",
        usage: "Darkest stop of the main diagonal background gradient.",
        kind: "color",
      },
      "--aero-bg-gradient-mid": {
        name: "Base gradient middle",
        usage: "Mid stop of the main diagonal background gradient (~42%).",
        kind: "color",
      },
      "--aero-bg-gradient-end": {
        name: "Base gradient end",
        usage: "Brightest stop of the main diagonal background gradient.",
        kind: "color",
      },
    },
    semanticFields: {
      "--foreground": {
        name: "Page text",
        usage: "Default body text on the aurora background (headings use accent-retro separately).",
      },
      "--muted-foreground": {
        name: "Secondary page text",
        usage:
          "Subtitles and helper copy on the background—e.g. Admin description, inactive nav tabs, settings hints.",
      },
      "--card-foreground": {
        name: "Text on glass cards",
        usage: "Primary text inside cards, panels, and popovers.",
      },
      "--primary-foreground": {
        name: "Text on primary buttons",
        usage: "Label color on filled primary actions.",
      },
      "--secondary-foreground": {
        name: "Text on secondary surfaces",
        usage: "Text on secondary buttons and secondary-tinted UI blocks.",
      },
      "--accent-foreground": {
        name: "Text on accent surfaces",
        usage: "Text on accent-tinted controls and highlights.",
      },
      "--popover-foreground": {
        name: "Popover text",
        usage: "Text inside dropdowns, popovers, and the color picker.",
      },
      "--background": {
        name: "Page base color",
        usage: "Fallback page fill behind the gradient (visible while loading or in gaps).",
      },
    },
  },
  steam: {
    locked: true,
    defaults: FRUTIGER_AERO_DEFAULT_PALETTE,
    gradientDefaults: FRUTIGER_AERO_DEFAULT_GRADIENTS,
    semanticDefaults: FRUTIGER_AERO_DEFAULT_SEMANTICS,
    fields: {
      "--aero-cerulean": { name: "", usage: "" },
      "--aero-persian-green": { name: "", usage: "" },
      "--aero-robin-egg": { name: "", usage: "" },
      "--aero-cream-can": { name: "", usage: "" },
      "--aero-flamingo": { name: "", usage: "" },
    },
    gradientFields: {
      "--aero-bg-glow-top": { name: "", usage: "", kind: "color" },
      "--aero-bg-glow-right": { name: "", usage: "", kind: "color" },
      "--aero-bg-glow-bottom": { name: "", usage: "", kind: "color" },
      "--aero-bg-glow-top-opacity": { name: "", usage: "", kind: "opacity" },
      "--aero-bg-glow-right-opacity": { name: "", usage: "", kind: "opacity" },
      "--aero-bg-glow-bottom-opacity": { name: "", usage: "", kind: "opacity" },
      "--aero-bg-gradient-start": { name: "", usage: "", kind: "color" },
      "--aero-bg-gradient-mid": { name: "", usage: "", kind: "color" },
      "--aero-bg-gradient-end": { name: "", usage: "", kind: "color" },
    },
    semanticFields: {
      "--foreground": { name: "", usage: "" },
      "--muted-foreground": { name: "", usage: "" },
      "--card-foreground": { name: "", usage: "" },
      "--primary-foreground": { name: "", usage: "" },
      "--secondary-foreground": { name: "", usage: "" },
      "--accent-foreground": { name: "", usage: "" },
      "--popover-foreground": { name: "", usage: "" },
      "--background": { name: "", usage: "" },
    },
  },
};

export const ALL_SEMANTIC_CSS_VARS: SemanticCssVar[] = [
  "--foreground",
  "--muted-foreground",
  "--card-foreground",
  "--primary-foreground",
  "--secondary-foreground",
  "--accent-foreground",
  "--popover-foreground",
  "--background",
];

export const ALL_PALETTE_CSS_VARS: PaletteCssVar[] = [
  "--aero-cerulean",
  "--aero-persian-green",
  "--aero-robin-egg",
  "--aero-cream-can",
  "--aero-flamingo",
];

export const ALL_GRADIENT_CSS_VARS: GradientCssVar[] = [
  "--aero-bg-glow-top",
  "--aero-bg-glow-right",
  "--aero-bg-glow-bottom",
  "--aero-bg-glow-top-opacity",
  "--aero-bg-glow-right-opacity",
  "--aero-bg-glow-bottom-opacity",
  "--aero-bg-gradient-start",
  "--aero-bg-gradient-mid",
  "--aero-bg-gradient-end",
];

type StoredThemeLabEntry = {
  palette?: Partial<ThemePalette>;
  gradients?: Partial<ThemeGradients>;
  semantics?: Partial<ThemeSemantics>;
};

type StoredThemeLab = Partial<Record<ThemeId, StoredThemeLabEntry | Partial<ThemePalette>>>;

function isLegacyPaletteEntry(
  value: StoredThemeLabEntry | Partial<ThemePalette>,
): value is Partial<ThemePalette> {
  return value != null && "--aero-cerulean" in value;
}

function readRawStore(): StoredThemeLab {
  if (typeof window === "undefined") {
    return {};
  }
  try {
    const raw = localStorage.getItem(THEME_LAB_STORAGE_KEY);
    if (!raw) {
      return {};
    }
    return JSON.parse(raw) as StoredThemeLab;
  } catch {
    return {};
  }
}

function normalizeStoreEntry(
  value: StoredThemeLabEntry | Partial<ThemePalette> | undefined,
): StoredThemeLabEntry {
  if (!value) {
    return {};
  }
  if (isLegacyPaletteEntry(value)) {
    return { palette: value };
  }
  return value;
}

export function isThemeLabEditable(themeId: ThemeId): boolean {
  const config = THEME_PALETTE_CONFIG[themeId];
  return config != null && !config.locked;
}

export function getDefaultPalette(themeId: ThemeId): ThemePalette | null {
  const config = THEME_PALETTE_CONFIG[themeId];
  if (!config || config.locked) {
    return null;
  }
  return { ...config.defaults };
}

export function getDefaultGradients(themeId: ThemeId): ThemeGradients | null {
  const config = THEME_PALETTE_CONFIG[themeId];
  if (!config || config.locked) {
    return null;
  }
  return { ...config.gradientDefaults };
}

export function getDefaultSemantics(themeId: ThemeId): ThemeSemantics | null {
  const config = THEME_PALETTE_CONFIG[themeId];
  if (!config || config.locked) {
    return null;
  }
  return { ...config.semanticDefaults };
}

export function getDefaultThemeLab(themeId: ThemeId): ThemeLabOverrides | null {
  const palette = getDefaultPalette(themeId);
  const gradients = getDefaultGradients(themeId);
  const semantics = getDefaultSemantics(themeId);
  if (!palette || !gradients || !semantics) {
    return null;
  }
  return { palette, gradients, semantics };
}

export function getEffectiveThemeLab(themeId: ThemeId): ThemeLabOverrides | null {
  const paletteDefaults = getDefaultPalette(themeId);
  const gradientDefaults = getDefaultGradients(themeId);
  const semanticDefaults = getDefaultSemantics(themeId);
  if (!paletteDefaults || !gradientDefaults || !semanticDefaults) {
    return null;
  }
  const stored = normalizeStoreEntry(readRawStore()[themeId]);
  return {
    palette: { ...paletteDefaults, ...stored.palette },
    gradients: { ...gradientDefaults, ...stored.gradients },
    semantics: { ...semanticDefaults, ...stored.semantics },
  };
}

export function getEffectivePalette(themeId: ThemeId): ThemePalette | null {
  return getEffectiveThemeLab(themeId)?.palette ?? null;
}

export function getEffectiveGradients(themeId: ThemeId): ThemeGradients | null {
  return getEffectiveThemeLab(themeId)?.gradients ?? null;
}

export function saveThemeLabForTheme(themeId: ThemeId, overrides: ThemeLabOverrides): void {
  const all = readRawStore();
  all[themeId] = {
    palette: overrides.palette,
    gradients: overrides.gradients,
    semantics: overrides.semantics,
  };
  try {
    localStorage.setItem(THEME_LAB_STORAGE_KEY, JSON.stringify(all));
  } catch {
    // ignore
  }
}

export function savePaletteForTheme(themeId: ThemeId, palette: ThemePalette): void {
  const current = getEffectiveThemeLab(themeId);
  if (!current) {
    return;
  }
  saveThemeLabForTheme(themeId, { ...current, palette });
}

export function saveGradientsForTheme(themeId: ThemeId, gradients: ThemeGradients): void {
  const current = getEffectiveThemeLab(themeId);
  if (!current) {
    return;
  }
  saveThemeLabForTheme(themeId, { ...current, gradients });
}

export function clearThemeLabForTheme(themeId: ThemeId): void {
  const all = readRawStore();
  delete all[themeId];
  try {
    localStorage.setItem(THEME_LAB_STORAGE_KEY, JSON.stringify(all));
  } catch {
    // ignore
  }
}

export function clearPaletteForTheme(themeId: ThemeId): void {
  clearThemeLabForTheme(themeId);
}

const ALL_THEME_LAB_CSS_VARS = [
  ...ALL_PALETTE_CSS_VARS,
  ...ALL_GRADIENT_CSS_VARS,
  ...ALL_SEMANTIC_CSS_VARS,
];

export function clearThemeLabInlineStyles(): void {
  if (typeof document === "undefined") {
    return;
  }
  const root = document.documentElement;
  for (const name of ALL_THEME_LAB_CSS_VARS) {
    root.style.removeProperty(name);
  }
}

export function applyThemeLabPalette(themeId: ThemeId): void {
  clearThemeLabInlineStyles();
  if (!isThemeLabEditable(themeId)) {
    return;
  }
  const lab = getEffectiveThemeLab(themeId);
  if (!lab) {
    return;
  }
  const root = document.documentElement;
  for (const [name, value] of Object.entries(lab.palette)) {
    root.style.setProperty(name, value);
  }
  for (const [name, value] of Object.entries(lab.gradients)) {
    root.style.setProperty(name, formatGradientCssValue(name as GradientCssVar, value));
  }
  for (const [name, value] of Object.entries(lab.semantics)) {
    root.style.setProperty(name, value);
  }
}

function formatGradientCssValue(name: GradientCssVar, value: string): string {
  if (name.endsWith("-opacity")) {
    return value.trim().endsWith("%") ? value.trim() : `${value.trim()}%`;
  }
  return value;
}

export function exportThemeLabCss(themeId: ThemeId, lab: ThemeLabOverrides): string {
  const lines = [
    ...Object.entries(lab.palette).map(([name, value]) => `  ${name}: ${value};`),
    ...Object.entries(lab.gradients).map(([name, value]) => {
      const formatted = formatGradientCssValue(name as GradientCssVar, value);
      return `  ${name}: ${formatted};`;
    }),
    ...Object.entries(lab.semantics).map(([name, value]) => `  ${name}: ${value};`),
  ];
  const varsBlock = `[data-theme="${themeId}"] {\n${lines.join("\n")}\n}`;
  const bodyNote = `
/* Page background uses these variables in globals.css:
[data-theme="${themeId}"] body {
  background-image:
    radial-gradient(ellipse 120% 80% at 10% 0%, color-mix(in srgb, var(--aero-bg-glow-top) var(--aero-bg-glow-top-opacity), transparent), transparent 55%),
    ...
}
*/`;
  return `${varsBlock}${bodyNote}`;
}

export function exportThemePaletteCss(themeId: ThemeId, palette: ThemePalette): string {
  const lab = getEffectiveThemeLab(themeId);
  const gradients = lab?.gradients ?? getDefaultGradients(themeId);
  const semantics = lab?.semantics ?? getDefaultSemantics(themeId);
  if (!gradients || !semantics) {
    return exportThemeLabCss(themeId, {
      palette,
      gradients: FRUTIGER_AERO_DEFAULT_GRADIENTS,
      semantics: FRUTIGER_AERO_DEFAULT_SEMANTICS,
    });
  }
  return exportThemeLabCss(themeId, { palette, gradients, semantics });
}

const HEX_6 = /^#[0-9A-Fa-f]{6}$/;

export function normalizeHexColor(raw: string): string | null {
  const trimmed = raw.trim();
  if (!trimmed) {
    return null;
  }
  const withHash = trimmed.startsWith("#") ? trimmed : `#${trimmed}`;
  if (!HEX_6.test(withHash)) {
    return null;
  }
  return withHash.toUpperCase();
}

export function normalizeOpacityPercent(raw: string): string | null {
  const trimmed = raw.trim().replace(/%$/, "");
  if (!/^\d{1,3}$/.test(trimmed)) {
    return null;
  }
  const value = Number(trimmed);
  if (value < 0 || value > 100) {
    return null;
  }
  return String(value);
}

export function hslStringToHex(hsl: string): string {
  const nums = hsl.match(/\d+(\.\d+)?/g)?.map(Number) ?? [0, 0, 0];
  const h = nums[0] ?? 0;
  const s = nums[1] ?? 0;
  const l = nums[2] ?? 0;
  return hslComponentsToHex(h, s, l);
}

function hslComponentsToHex(h: number, s: number, l: number): string {
  const lightness = l / 100;
  const a = (s * Math.min(lightness, 1 - lightness)) / 100;
  const f = (n: number) => {
    const k = (n + h / 30) % 12;
    const color = lightness - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
    return Math.round(255 * color)
      .toString(16)
      .padStart(2, "0");
  };
  return `#${f(0)}${f(8)}${f(4)}`.toUpperCase();
}
