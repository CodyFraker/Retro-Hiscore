jest.mock("next/headers", () => ({
  cookies: jest.fn(),
}));

jest.mock("next-auth/jwt", () => ({
  getToken: jest.fn(),
}));

jest.mock("@/lib/invalid-session", () => ({
  isApiAccessTokenExpired: jest.fn(),
}));

const mockCookies = jest.requireMock("next/headers").cookies as jest.Mock;
const mockGetToken = jest.requireMock("next-auth/jwt").getToken as jest.Mock;
const mockIsExpired = jest.requireMock("@/lib/invalid-session").isApiAccessTokenExpired as jest.Mock;

import { hasValidServerApiAccess } from "@/lib/server-api-access-token";

describe("hasValidServerApiAccess", () => {
  beforeEach(() => {
    mockCookies.mockReset();
    mockGetToken.mockReset();
    mockIsExpired.mockReset();
    process.env.AUTH_SECRET = "test-secret";
    mockCookies.mockResolvedValue({
      toString: () => "session=abc",
    });
    mockIsExpired.mockReturnValue(false);
  });

  afterEach(() => {
    delete process.env.AUTH_SECRET;
  });

  it("returns true when JWT contains a non-expired api access token", async () => {
    // Arrange
    mockGetToken.mockResolvedValue({
      apiAccessToken: "valid-token",
    });

    // Act
    const result = await hasValidServerApiAccess();

    // Assert
    expect(result).toBe(true);
  });

  it("returns false when api access token is missing", async () => {
    // Arrange
    mockGetToken.mockResolvedValue({});

    // Act
    const result = await hasValidServerApiAccess();

    // Assert
    expect(result).toBe(false);
  });
});
