const mockSign = jest.fn().mockResolvedValue("signed-token");

jest.mock("jose", () => ({
  SignJWT: jest.fn().mockImplementation(() => ({
    setProtectedHeader: () => ({
      setSubject: () => ({
        setIssuer: (issuer: string) => ({
          setAudience: (audience: string) => ({
            setJti: () => ({
              setIssuedAt: () => ({
                setExpirationTime: () => ({
                  sign: async () => {
                    mockSign({ issuer, audience });
                    return "signed-token";
                  },
                }),
              }),
            }),
          }),
        }),
      }),
    }),
  })),
  decodeJwt: () => ({ iss: "retro-hiscore-web", aud: "retro-hiscore-api" }),
}));

import { createApiAccessToken } from "@/lib/api-access-token";

describe("createApiAccessToken", () => {
  beforeEach(() => {
    process.env.AUTH_SECRET = "test-secret";
    mockSign.mockClear();
  });

  afterEach(() => {
    delete process.env.AUTH_SECRET;
    delete process.env.AUTH_JWT_ISSUER;
    delete process.env.AUTH_JWT_AUDIENCE;
  });

  it("includes issuer and audience claims", async () => {
    // Act
    await createApiAccessToken("123", "member");

    // Assert
    expect(mockSign).toHaveBeenCalledWith({
      issuer: "retro-hiscore-web",
      audience: "retro-hiscore-api",
    });
  });
});
