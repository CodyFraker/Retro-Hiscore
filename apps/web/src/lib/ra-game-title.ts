export type RaGameTitleModTag = "homebrew" | "hack";

export function parseRaGameTitle(rawTitle: string): {
  displayTitle: string;
  modTags: RaGameTitleModTag[];
} {
  const modTags: RaGameTitleModTag[] = [];
  if (/~Homebrew~/i.test(rawTitle)) {
    modTags.push("homebrew");
  }
  if (/~Hack~/i.test(rawTitle)) {
    modTags.push("hack");
  }

  const displayTitle = rawTitle
    .replace(/~Homebrew~/gi, "")
    .replace(/~Hack~/gi, "")
    .replace(/\s+/g, " ")
    .trim();

  return { displayTitle, modTags };
}
