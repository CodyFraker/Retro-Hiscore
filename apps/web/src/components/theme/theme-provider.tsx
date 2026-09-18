"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { toast } from "sonner";
import { saveMemberUiThemeAction } from "@/lib/actions/settings";
import {
  applyThemeToDocument,
  readStoredTheme,
  type ThemeId,
  writeStoredTheme,
} from "@/lib/theme";

type ThemeContextValue = {
  theme: ThemeId;
  setTheme: (theme: ThemeId) => void;
  isSaving: boolean;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);

type Props = {
  children: ReactNode;
  initialTheme: ThemeId;
  syncToAccount: boolean;
};

export function ThemeProvider({ children, initialTheme, syncToAccount }: Props) {
  const [theme, setThemeState] = useState<ThemeId>(initialTheme);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (syncToAccount) {
      setThemeState(initialTheme);
      applyThemeToDocument(initialTheme);
      writeStoredTheme(initialTheme);
      return;
    }
    const stored = readStoredTheme() ?? initialTheme;
    setThemeState(stored);
    applyThemeToDocument(stored);
  }, [initialTheme, syncToAccount]);

  const setTheme = useCallback(
    async (next: ThemeId) => {
      const previous = theme;
      setThemeState(next);
      applyThemeToDocument(next);
      writeStoredTheme(next);

      if (!syncToAccount) {
        return;
      }

      setIsSaving(true);
      try {
        const result = await saveMemberUiThemeAction(next);
        if (!result.ok) {
          setThemeState(previous);
          applyThemeToDocument(previous);
          writeStoredTheme(previous);
          toast.error(result.error);
        }
      } finally {
        setIsSaving(false);
      }
    },
    [syncToAccount, theme],
  );

  const value = useMemo(
    () => ({ theme, setTheme, isSaving }),
    [theme, setTheme, isSaving],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) {
    throw new Error("useTheme must be used within ThemeProvider");
  }
  return ctx;
}
