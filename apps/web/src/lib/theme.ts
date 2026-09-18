import { applyThemeLabPalette } from "@/lib/theme-lab";

export const THEME_IDS = ["steam", "frutiger-aero"] as const;

export type ThemeId = (typeof THEME_IDS)[number];

export const DEFAULT_THEME: ThemeId = "steam";

export const STORAGE_KEY = "retro-hiscore-theme";

export function isThemeId(value: string): value is ThemeId {
  return (THEME_IDS as readonly string[]).includes(value);
}

export function resolveTheme(candidate: string | null | undefined): ThemeId {
  if (candidate && isThemeId(candidate)) {
    return candidate;
  }
  return DEFAULT_THEME;
}

export const THEME_LABELS: Record<ThemeId, string> = {
  steam: "Steam",
  "frutiger-aero": "Frutiger Aero",
};

export function applyThemeToDocument(theme: ThemeId): void {
  document.documentElement.dataset.theme = theme;
  applyThemeLabPalette(theme);
}

export function readStoredTheme(): ThemeId | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && isThemeId(stored)) {
      return stored;
    }
  } catch {
    return null;
  }
  return null;
}

export function writeStoredTheme(theme: ThemeId): void {
  try {
    localStorage.setItem(STORAGE_KEY, theme);
  } catch {
    // ignore quota / private mode
  }
}
