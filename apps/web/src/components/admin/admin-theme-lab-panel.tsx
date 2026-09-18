"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";
import { ColorPicker } from "@/components/ui/color-picker";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useTheme } from "@/components/theme/theme-provider";
import { THEME_IDS, THEME_LABELS, type ThemeId } from "@/lib/theme";
import {
  ALL_GRADIENT_CSS_VARS,
  ALL_PALETTE_CSS_VARS,
  ALL_SEMANTIC_CSS_VARS,
  applyThemeLabPalette,
  clearThemeLabForTheme,
  exportThemeLabCss,
  getEffectiveThemeLab,
  getDefaultThemeLab,
  hslStringToHex,
  isThemeLabEditable,
  normalizeHexColor,
  normalizeOpacityPercent,
  saveThemeLabForTheme,
  THEME_PALETTE_CONFIG,
  type GradientCssVar,
  type PaletteCssVar,
  type SemanticCssVar,
  type ThemeLabOverrides,
} from "@/lib/theme-lab";

type HexTarget = "palette" | "gradients" | "semantics";
type HexKey = PaletteCssVar | GradientCssVar | SemanticCssVar;
import { cn } from "@/lib/utils";

export function AdminThemeLabPanel() {
  const { theme, setTheme } = useTheme();
  const [lab, setLab] = useState<ThemeLabOverrides | null>(null);
  const [exportText, setExportText] = useState("");
  const [hexDraft, setHexDraft] = useState<Partial<Record<string, string>>>({});

  const editableThemes = useMemo(
    () => THEME_IDS.filter((id) => isThemeLabEditable(id)),
    [],
  );

  const [editingTheme, setEditingTheme] = useState<ThemeId>(
    () => editableThemes[0] ?? "frutiger-aero",
  );

  const loadLab = useCallback((themeId: ThemeId) => {
    const effective = getEffectiveThemeLab(themeId);
    setLab(effective);
    setHexDraft({});
    if (effective) {
      setExportText(exportThemeLabCss(themeId, effective));
    } else {
      setExportText("");
    }
  }, []);

  useEffect(() => {
    if (isThemeLabEditable(editingTheme)) {
      loadLab(editingTheme);
    }
  }, [editingTheme, loadLab]);

  const persistLab = (themeId: ThemeId, next: ThemeLabOverrides) => {
    setLab(next);
    saveThemeLabForTheme(themeId, next);
    setExportText(exportThemeLabCss(themeId, next));
    if (theme === themeId) {
      applyThemeLabPalette(themeId);
    }
  };

  const handlePaletteColorChange = (cssVar: PaletteCssVar, hslColor: string) => {
    if (!lab) {
      return;
    }
    const hex = hslStringToHex(hslColor);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[cssVar];
      return next;
    });
    persistLab(editingTheme, { ...lab, palette: { ...lab.palette, [cssVar]: hex } });
  };

  const handleGradientColorChange = (cssVar: GradientCssVar, hslColor: string) => {
    if (!lab) {
      return;
    }
    const hex = hslStringToHex(hslColor);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[cssVar];
      return next;
    });
    persistLab(editingTheme, {
      ...lab,
      gradients: { ...lab.gradients, [cssVar]: hex },
    });
  };

  const handleSemanticColorChange = (cssVar: SemanticCssVar, hslColor: string) => {
    if (!lab) {
      return;
    }
    const hex = hslStringToHex(hslColor);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[cssVar];
      return next;
    });
    persistLab(editingTheme, {
      ...lab,
      semantics: { ...lab.semantics, [cssVar]: hex },
    });
  };

  const applyHexToLab = (key: HexKey, normalized: string, target: HexTarget) => {
    if (!lab) {
      return;
    }
    if (target === "palette") {
      persistLab(editingTheme, {
        ...lab,
        palette: { ...lab.palette, [key as PaletteCssVar]: normalized },
      });
      return;
    }
    if (target === "gradients") {
      persistLab(editingTheme, {
        ...lab,
        gradients: { ...lab.gradients, [key as GradientCssVar]: normalized },
      });
      return;
    }
    persistLab(editingTheme, {
      ...lab,
      semantics: { ...lab.semantics, [key as SemanticCssVar]: normalized },
    });
  };

  const readHexFromLab = (key: HexKey, target: HexTarget): string => {
    if (!lab) {
      return "";
    }
    if (target === "palette") {
      return lab.palette[key as PaletteCssVar];
    }
    if (target === "gradients") {
      return lab.gradients[key as GradientCssVar];
    }
    return lab.semantics[key as SemanticCssVar];
  };

  const handleHexInputChange = (key: HexKey, raw: string, target: HexTarget) => {
    if (!lab) {
      return;
    }
    setHexDraft((draft) => ({ ...draft, [key]: raw }));
    const normalized = normalizeHexColor(raw);
    if (!normalized) {
      return;
    }
    applyHexToLab(key, normalized, target);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[key];
      return next;
    });
  };

  const handleHexInputBlur = (key: HexKey, raw: string, target: HexTarget) => {
    if (!lab) {
      return;
    }
    const normalized = normalizeHexColor(raw);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[key];
      return next;
    });
    if (!normalized || normalized === readHexFromLab(key, target)) {
      return;
    }
    applyHexToLab(key, normalized, target);
  };

  const handleOpacityChange = (cssVar: GradientCssVar, raw: string) => {
    if (!lab) {
      return;
    }
    setHexDraft((draft) => ({ ...draft, [cssVar]: raw }));
    const normalized = normalizeOpacityPercent(raw);
    if (normalized) {
      persistLab(editingTheme, {
        ...lab,
        gradients: { ...lab.gradients, [cssVar]: normalized },
      });
      setHexDraft((draft) => {
        const next = { ...draft };
        delete next[cssVar];
        return next;
      });
    }
  };

  const handleOpacityBlur = (cssVar: GradientCssVar, raw: string) => {
    if (!lab) {
      return;
    }
    const normalized = normalizeOpacityPercent(raw);
    setHexDraft((draft) => {
      const next = { ...draft };
      delete next[cssVar];
      return next;
    });
    if (normalized && normalized !== lab.gradients[cssVar]) {
      persistLab(editingTheme, {
        ...lab,
        gradients: { ...lab.gradients, [cssVar]: normalized },
      });
    }
  };

  const handleReset = () => {
    const defaults = getDefaultThemeLab(editingTheme);
    if (!defaults) {
      return;
    }
    clearThemeLabForTheme(editingTheme);
    setLab(defaults);
    setExportText(exportThemeLabCss(editingTheme, defaults));
    if (theme === editingTheme) {
      applyThemeLabPalette(editingTheme);
    }
    toast.success("Theme lab reset to CSS defaults.");
  };

  const handleCopyExport = async () => {
    try {
      await navigator.clipboard.writeText(exportText);
      toast.success("CSS copied to clipboard.");
    } catch {
      toast.error("Could not copy to clipboard.");
    }
  };

  if (editableThemes.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">No editable themes are configured for the lab.</p>
    );
  }

  const config = THEME_PALETTE_CONFIG[editingTheme];

  const gradientColorVars = ALL_GRADIENT_CSS_VARS.filter(
    (v) => config?.gradientFields[v].kind === "color",
  );
  const gradientOpacityVars = ALL_GRADIENT_CSS_VARS.filter(
    (v) => config?.gradientFields[v].kind === "opacity",
  );

  return (
    <div className="space-y-8">
      <section className="space-y-3">
        <h2 className="font-heading text-base font-medium tracking-wide text-primary uppercase">
          Theme preview
        </h2>
        <p className="text-sm text-muted-foreground">
          Pick a theme to edit its palette and background. Changes apply live and are stored in
          localStorage only (not your account). Steam is fixed in CSS.
        </p>
        <div className="flex flex-wrap gap-2">
          {THEME_IDS.map((id) => {
            const locked = !isThemeLabEditable(id);
            const selected = editingTheme === id;
            return (
              <Button
                key={id}
                type="button"
                variant={selected ? "default" : "outline"}
                size="sm"
                disabled={locked}
                onClick={() => {
                  if (!locked) {
                    setEditingTheme(id);
                    loadLab(id);
                    void setTheme(id);
                  }
                }}
              >
                {THEME_LABELS[id]}
                {locked ? " (locked)" : ""}
              </Button>
            );
          })}
        </div>
      </section>

      {lab && config ? (
        <>
          <section className="space-y-4 rounded border border-border p-5">
            <h3 className="font-medium">{THEME_LABELS[editingTheme]} UI palette</h3>
            <div className="grid gap-6 sm:grid-cols-2">
              {ALL_PALETTE_CSS_VARS.map((cssVar) => {
                const field = config.fields[cssVar];
                return (
                  <div key={cssVar} className="space-y-2">
                    <div className="space-y-1">
                      <Label className="leading-snug">{field.usage}</Label>
                      <p className="text-xs text-muted-foreground">
                        {field.name}
                        <span className="font-mono"> · {cssVar}</span>
                      </p>
                    </div>
                    <ColorPicker
                      color={lab.palette[cssVar]}
                      onChange={(c) => handlePaletteColorChange(cssVar, c)}
                    />
                    <div className="space-y-1">
                      <Label htmlFor={`hex-${cssVar}`} className="text-xs text-muted-foreground">
                        Hex
                      </Label>
                      <Input
                        id={`hex-${cssVar}`}
                        type="text"
                        autoComplete="off"
                        spellCheck={false}
                        className="font-mono text-sm"
                        placeholder="#RRGGBB"
                        value={hexDraft[cssVar] ?? lab.palette[cssVar]}
                        onChange={(e) => handleHexInputChange(cssVar, e.target.value, "palette")}
                        onBlur={(e) => handleHexInputBlur(cssVar, e.target.value, "palette")}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          </section>

          <section className="space-y-4 rounded border border-border p-5">
            <h3 className="font-medium">{THEME_LABELS[editingTheme]} text &amp; surfaces</h3>
            <p className="text-sm text-muted-foreground">
              Shadcn semantic colors. Secondary page text uses{" "}
              <span className="font-mono">--muted-foreground</span> (e.g. text under page
              titles). Card descriptions use a darker mix of{" "}
              <span className="font-mono">--card-foreground</span> in CSS.
            </p>
            <div className="grid gap-6 sm:grid-cols-2">
              {ALL_SEMANTIC_CSS_VARS.map((cssVar) => {
                const field = config.semanticFields[cssVar];
                return (
                  <div key={cssVar} className="space-y-2">
                    <div className="space-y-1">
                      <Label className="leading-snug">{field.usage}</Label>
                      <p className="text-xs text-muted-foreground">
                        {field.name}
                        <span className="font-mono"> · {cssVar}</span>
                      </p>
                    </div>
                    <ColorPicker
                      color={lab.semantics[cssVar]}
                      onChange={(c) => handleSemanticColorChange(cssVar, c)}
                    />
                    <div className="space-y-1">
                      <Label htmlFor={`hex-${cssVar}`} className="text-xs text-muted-foreground">
                        Hex
                      </Label>
                      <Input
                        id={`hex-${cssVar}`}
                        type="text"
                        autoComplete="off"
                        spellCheck={false}
                        className="font-mono text-sm"
                        placeholder="#RRGGBB"
                        value={hexDraft[cssVar] ?? lab.semantics[cssVar]}
                        onChange={(e) => handleHexInputChange(cssVar, e.target.value, "semantics")}
                        onBlur={(e) => handleHexInputBlur(cssVar, e.target.value, "semantics")}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          </section>

          <section className="space-y-4 rounded border border-border p-5">
            <h3 className="font-medium">{THEME_LABELS[editingTheme]} background gradients</h3>
            <p className="text-sm text-muted-foreground">
              Aurora radials and the base diagonal gradient behind the page. Opacity values are
              percentages (0–100).
            </p>
            <div className="grid gap-6 sm:grid-cols-2">
              {gradientColorVars.map((cssVar) => {
                const field = config.gradientFields[cssVar];
                return (
                  <div key={cssVar} className="space-y-2">
                    <div className="space-y-1">
                      <Label className="leading-snug">{field.usage}</Label>
                      <p className="text-xs text-muted-foreground">
                        {field.name}
                        <span className="font-mono"> · {cssVar}</span>
                      </p>
                    </div>
                    <ColorPicker
                      color={lab.gradients[cssVar]}
                      onChange={(c) => handleGradientColorChange(cssVar, c)}
                    />
                    <div className="space-y-1">
                      <Label htmlFor={`hex-${cssVar}`} className="text-xs text-muted-foreground">
                        Hex
                      </Label>
                      <Input
                        id={`hex-${cssVar}`}
                        type="text"
                        autoComplete="off"
                        spellCheck={false}
                        className="font-mono text-sm"
                        placeholder="#RRGGBB"
                        value={hexDraft[cssVar] ?? lab.gradients[cssVar]}
                        onChange={(e) => handleHexInputChange(cssVar, e.target.value, "gradients")}
                        onBlur={(e) => handleHexInputBlur(cssVar, e.target.value, "gradients")}
                      />
                    </div>
                  </div>
                );
              })}
              {gradientOpacityVars.map((cssVar) => {
                const field = config.gradientFields[cssVar];
                return (
                  <div key={cssVar} className="space-y-2">
                    <div className="space-y-1">
                      <Label className="leading-snug">{field.usage}</Label>
                      <p className="text-xs text-muted-foreground">
                        {field.name}
                        <span className="font-mono"> · {cssVar}</span>
                      </p>
                    </div>
                    <Input
                      type="text"
                      inputMode="numeric"
                      autoComplete="off"
                      className="font-mono text-sm"
                      placeholder="0–100"
                      value={hexDraft[cssVar] ?? lab.gradients[cssVar]}
                      onChange={(e) => handleOpacityChange(cssVar, e.target.value)}
                      onBlur={(e) => handleOpacityBlur(cssVar, e.target.value)}
                    />
                  </div>
                );
              })}
            </div>
          </section>

          <div className="flex flex-wrap gap-2">
            <Button type="button" variant="outline" size="sm" onClick={handleReset}>
              Reset to defaults
            </Button>
            <Button type="button" variant="secondary" size="sm" onClick={handleCopyExport}>
              Copy CSS block
            </Button>
          </div>
        </>
      ) : null}

      <section className="space-y-2">
        <h3 className="text-sm font-medium">Export for globals.css</h3>
        <pre
          className={cn(
            "max-h-96 overflow-auto rounded-md border border-border bg-muted/40 p-4 font-mono text-xs",
          )}
        >
          {exportText || "Adjust colors to generate CSS."}
        </pre>
      </section>
    </div>
  );
}
