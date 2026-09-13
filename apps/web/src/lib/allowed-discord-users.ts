export function getAllowedDiscordUserIds(): Set<string> {
  const raw = process.env.AUTH_ALLOWED_DISCORD_USER_IDS ?? "";
  return new Set(
    raw
      .split(",")
      .map((id) => id.trim())
      .filter(Boolean),
  );
}

export function isDiscordUserAllowlisted(discordUserId: string | undefined | null): boolean {
  if (!discordUserId) {
    return false;
  }

  const allowed = getAllowedDiscordUserIds();
  return allowed.size > 0 && allowed.has(discordUserId);
}
