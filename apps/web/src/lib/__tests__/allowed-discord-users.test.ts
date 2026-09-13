import { getAllowedDiscordUserIds, isDiscordUserAllowlisted } from "@/lib/allowed-discord-users";

describe("allowed-discord-users", () => {
  const original = process.env.AUTH_ALLOWED_DISCORD_USER_IDS;

  afterEach(() => {
    process.env.AUTH_ALLOWED_DISCORD_USER_IDS = original;
  });

  it("parses comma-separated Discord user ids", () => {
    // Arrange
    process.env.AUTH_ALLOWED_DISCORD_USER_IDS = " 111 , 222 ,333 ";

    // Act
    const allowed = getAllowedDiscordUserIds();

    // Assert
    expect(allowed.has("111")).toBe(true);
    expect(allowed.has("222")).toBe(true);
    expect(allowed.has("333")).toBe(true);
    expect(allowed.size).toBe(3);
  });

  it("rejects users not on the allowlist", () => {
    // Arrange
    process.env.AUTH_ALLOWED_DISCORD_USER_IDS = "111";

    // Act
    const allowed = isDiscordUserAllowlisted("222");

    // Assert
    expect(allowed).toBe(false);
  });

  it("accepts allowlisted users", () => {
    // Arrange
    process.env.AUTH_ALLOWED_DISCORD_USER_IDS = "111";

    // Act
    const allowed = isDiscordUserAllowlisted("111");

    // Assert
    expect(allowed).toBe(true);
  });
});
