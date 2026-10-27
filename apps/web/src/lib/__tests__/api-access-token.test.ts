import type { JWT } from "next-auth/jwt";

const mockSignJwt = jest.fn();
const mockIsExpired = jest.fn();

jest.mock("jose", () => ({
  SignJWT: jest.fn().mockImplementation(() => ({
    setProtectedHeader: () => ({
      setSubject: () => ({
        setJti: () => ({
          setIssuedAt: () => ({
            setExpirationTime: (value: string) => {
              mockSignJwt(value);
              return { sign: async () => "new-api-token" };
            },
          }),
        }),
      }),
    }),
  })),
}));

jest.mock("@/lib/invalid-session", () => ({
  isApiAccessTokenExpired: (...args: unknown[]) => mockIsExpired(...args),
}));

import { ensureApiAccessTokenOnJwt } from "@/lib/api-access-token";
import { SESSION_MAX_AGE_SECONDS } from "@/lib/session-duration";

describe("ensureApiAccessTokenOnJwt", () => {
  const originalSecret = process.env.AUTH_SECRET;

  beforeEach(() => {
    mockSignJwt.mockClear();
    mockIsExpired.mockReset();
    process.env.AUTH_SECRET = "test-secret";
  });

  afterAll(() => {
    process.env.AUTH_SECRET = originalSecret;
  });

  it("returns the token unchanged when discord identity is missing", async () => {
    // Arrange
    const token: JWT = { apiAccessToken: "old" };

    // Act
    const result = await ensureApiAccessTokenOnJwt(token);

    // Assert
    expect(result.apiAccessToken).toBe("old");
    expect(mockSignJwt).not.toHaveBeenCalled();
  });

  it("returns the token unchanged when the API access token is still valid", async () => {
    // Arrange
    mockIsExpired.mockReturnValue(false);
    const token: JWT = {
      discordId: "123",
      discordUsername: "member",
      apiAccessToken: "valid-token",
    };

    // Act
    const result = await ensureApiAccessTokenOnJwt(token);

    // Assert
    expect(result.apiAccessToken).toBe("valid-token");
    expect(mockIsExpired).toHaveBeenCalledWith("valid-token");
    expect(mockSignJwt).not.toHaveBeenCalled();
  });

  it("issues a new API access token when the existing token is expired", async () => {
    // Arrange
    mockIsExpired.mockReturnValue(true);
    const token: JWT = {
      discordId: "123",
      discordUsername: "member",
      apiAccessToken: "expired-token",
    };

    // Act
    const result = await ensureApiAccessTokenOnJwt(token);

    // Assert
    expect(result.apiAccessToken).toBe("new-api-token");
    expect(mockSignJwt).toHaveBeenCalledWith(`${SESSION_MAX_AGE_SECONDS}s`);
  });
});
