import { isDiscordUserInvited } from "@/lib/sign-in-check";

describe("isDiscordUserInvited", () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    process.env.API_URL = "http://api.test";
    global.fetch = jest.fn().mockResolvedValue({ status: 204 });
  });

  afterEach(() => {
    global.fetch = originalFetch;
    delete process.env.API_URL;
    delete process.env.AUTH_SIGN_IN_SERVICE_KEY;
    delete process.env.AUTH_SECRET;
    delete process.env.NODE_ENV;
  });

  it("sends the dedicated sign-in service key header", async () => {
    // Arrange
    process.env.AUTH_SIGN_IN_SERVICE_KEY = "dedicated-key";

    // Act
    await isDiscordUserInvited("123");

    // Assert
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/auth/sign-in-check"),
      expect.objectContaining({
        headers: { "X-SignIn-Service-Key": "dedicated-key" },
      }),
    );
  });

  it("does not use AUTH_SECRET in production when dedicated key is missing", async () => {
    // Arrange
    process.env.NODE_ENV = "production";
    process.env.AUTH_SECRET = "auth-secret-only";

    // Act
    const invited = await isDiscordUserInvited("123");

    // Assert
    expect(invited).toBe(false);
    expect(global.fetch).not.toHaveBeenCalled();
  });
});
