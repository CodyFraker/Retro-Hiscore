jest.mock("@/lib/api-access-token", () => ({
  createApiAccessToken: jest.fn(),
  ensureApiAccessTokenOnJwt: jest.fn(async (token) => token),
}));
jest.mock("@/lib/sign-in-check", () => ({
  isDiscordUserInvited: jest.fn(),
}));
jest.mock("@/lib/sync-member-profile", () => ({
  syncMemberProfileOnLogin: jest.fn(),
}));
jest.mock("@/lib/telemetry/init-node-metrics", () => ({
  initNodeMetrics: jest.fn(),
}));
jest.mock("@/lib/telemetry/platform-metrics", () => ({
  recordAuthSignInRejectedNotInvited: jest.fn(),
  recordAuthSignInSuccess: jest.fn(),
}));
jest.mock("next-auth/providers/discord", () => ({
  __esModule: true,
  default: jest.fn(() => ({ id: "discord" })),
}));

import type { Session } from "next-auth";
import type { JWT } from "next-auth/jwt";
import { authOptions } from "@/lib/auth-options";

describe("authOptions session callback", () => {
  it("does not expose apiAccessToken on the session object", async () => {
    // Arrange
    const sessionCallback = authOptions.callbacks?.session;
    expect(sessionCallback).toBeDefined();
    const token: JWT = {
      apiAccessToken: "secret-api-jwt",
      discordId: "123",
      discordUsername: "user",
      needsOnboarding: false,
      isAdmin: true,
    };
    const session: Session = {
      expires: new Date(Date.now() + 60_000).toISOString(),
      user: { name: "user" },
    };

    // Act
    const result = await sessionCallback!({
      session,
      token,
      user: { id: "123" },
      newSession: undefined,
      trigger: "get",
    });

    // Assert
    expect(result).not.toHaveProperty("apiAccessToken");
    expect(result.needsOnboarding).toBe(false);
    expect(result.isAdmin).toBe(true);
    expect(result.user.discordId).toBe("123");
  });
});
