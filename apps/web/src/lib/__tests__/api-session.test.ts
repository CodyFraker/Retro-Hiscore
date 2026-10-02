import { getRedirectError, getURLFromRedirectError } from "next/dist/client/components/redirect";
import { isRedirectError } from "next/dist/client/components/redirect-error";

jest.mock("jose", () => ({
  decodeJwt: jest.fn(),
}));

const mockGetServerSession = jest.fn();
const mockRedirect = jest.fn();

jest.mock("next-auth", () => ({
  getServerSession: (...args: unknown[]) => mockGetServerSession(...args),
}));

jest.mock("next/navigation", () => ({
  redirect: (url: string) => mockRedirect(url),
}));

jest.mock("@/lib/auth-options", () => ({
  authOptions: {},
}));

import { getServerApiClient, getApiClient } from "@/lib/api";
import { signOutRedirectPath } from "@/lib/invalid-session";

describe("getServerApiClient", () => {
  beforeEach(() => {
    mockGetServerSession.mockReset();
    mockRedirect.mockReset();
    mockRedirect.mockImplementation((url: string) => {
      throw getRedirectError(url, "replace");
    });
    process.env.API_URL = "http://api.test";
  });

  afterEach(() => {
    delete process.env.API_URL;
  });

  it("redirects to sign out when the session has no API access token", async () => {
    // Arrange
    mockGetServerSession.mockResolvedValue({ user: { name: "member" } });

    // Act & Assert
    await getServerApiClient().catch((error: unknown) => {
      expect(isRedirectError(error)).toBe(true);
      expect(getURLFromRedirectError(error)).toBe(signOutRedirectPath());
    });
    expect(mockRedirect).toHaveBeenCalledWith(signOutRedirectPath());
  });
});

describe("getApiClient authorized fetch", () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    mockRedirect.mockReset();
    mockRedirect.mockImplementation((url: string) => {
      throw getRedirectError(url, "replace");
    });
    process.env.API_URL = "http://api.test";
  });

  afterEach(() => {
    global.fetch = originalFetch;
    delete process.env.API_URL;
  });

  it("redirects to sign out when the API responds with 401", async () => {
    // Arrange
    global.fetch = jest.fn().mockResolvedValue({
      status: 401,
      ok: false,
      statusText: "Unauthorized",
      text: async () => "Unauthorized",
    });
    const client = getApiClient({ accessToken: "stale-token" });

    // Act & Assert
    await client.getDashboard().catch((error: unknown) => {
      expect(isRedirectError(error)).toBe(true);
      expect(getURLFromRedirectError(error)).toBe(signOutRedirectPath());
    });
    expect(mockRedirect).toHaveBeenCalledWith(signOutRedirectPath());
  });

  it("does not redirect when the API responds with 401 without an access token", async () => {
    // Arrange
    global.fetch = jest.fn().mockResolvedValue({
      status: 401,
      ok: false,
      statusText: "Unauthorized",
      text: async () => "Unauthorized",
    });
    const client = getApiClient();

    // Act & Assert
    await expect(client.getDashboard()).rejects.toThrow(/^API 401:/);
    expect(mockRedirect).not.toHaveBeenCalled();
  });
});
