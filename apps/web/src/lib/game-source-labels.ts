const LABELS: Record<string, string> = {
  GoogleDrive: "Google Drive",
  Mega: "Mega",
  Mediafire: "Mediafire",
  Other: "Other",
};

export function gameSourceTypeLabel(sourceType: string, customLabel?: string | null) {
  if (sourceType === "Other" && customLabel) {
    return customLabel;
  }
  return LABELS[sourceType] ?? sourceType;
}

export const GAME_SOURCE_TYPE_OPTIONS = [
  { value: "GoogleDrive", label: "Google Drive" },
  { value: "Mega", label: "Mega" },
  { value: "Mediafire", label: "Mediafire" },
  { value: "Other", label: "Other" },
] as const;
