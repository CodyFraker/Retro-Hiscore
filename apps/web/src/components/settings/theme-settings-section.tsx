"use client";

import { useTheme } from "@/components/theme/theme-provider";
import { cn } from "@/lib/utils";
import { THEME_IDS, THEME_LABELS, type ThemeId } from "@/lib/theme";

const THEME_SWATCHES: Record<ThemeId, string[]> = {
  steam: ["#4c5844", "#968732", "#eff6ee"],
  "frutiger-aero": ["#5e9c9a", "#c8c3d5", "#fab700"],
};

type Props = {
  syncToAccount: boolean;
};

export function ThemeSettingsSection({ syncToAccount }: Props) {
  const { theme, setTheme, isSaving } = useTheme();

  return (
    <section className="space-y-4 rounded border border-border p-5">
      <div>
        <h2 className="font-heading text-base font-medium tracking-wide text-primary uppercase">
          Appearance
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Choose how Retro Hiscore looks on this device.
          {syncToAccount
            ? " Your choice is saved to your account and applies when you sign in elsewhere."
            : " Sign in with a linked member account to sync your theme across browsers."}
        </p>
      </div>
      <div className="grid gap-3 sm:grid-cols-2" role="radiogroup" aria-label="Site theme">
        {THEME_IDS.map((id) => {
          const selected = theme === id;
          return (
            <button
              key={id}
              type="button"
              role="radio"
              aria-checked={selected}
              disabled={isSaving}
              onClick={() => setTheme(id)}
              className={cn(
                "flex flex-col gap-3 rounded-md border p-4 text-left transition-colors",
                selected
                  ? "border-primary ring-2 ring-primary/30"
                  : "border-border hover:border-primary/50",
              )}
            >
              <div className="flex gap-1.5">
                {THEME_SWATCHES[id].map((color) => (
                  <span
                    key={color}
                    className="size-6 rounded-full border border-white/20 shadow-sm"
                    style={{ backgroundColor: color }}
                    aria-hidden
                  />
                ))}
              </div>
              <span className="font-medium">{THEME_LABELS[id]}</span>
            </button>
          );
        })}
      </div>
    </section>
  );
}
