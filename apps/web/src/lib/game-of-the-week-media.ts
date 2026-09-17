export function gameOfTheWeekImageUrl(imageIcon: string | null | undefined): string | null {
  if (!imageIcon) {
    return null;
  }
  return imageIcon.startsWith("http") ? imageIcon : `https://media.retroachievements.org${imageIcon}`;
}
