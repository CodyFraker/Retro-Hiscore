import { shouldUseSecureAuthCookies } from "@/lib/auth-cookies";

describe("shouldUseSecureAuthCookies", () => {
  const originalNodeEnv = process.env.NODE_ENV;

  afterEach(() => {
    process.env.NODE_ENV = originalNodeEnv;
    delete process.env.AUTH_URL;
    delete process.env.NEXTAUTH_URL;
  });

  it("returns true in production when AUTH_URL is https", () => {
    // Arrange
    process.env.NODE_ENV = "production";
    process.env.AUTH_URL = "https://example.com";

    // Act
    const result = shouldUseSecureAuthCookies();

    // Assert
    expect(result).toBe(true);
  });

  it("returns false for local http development", () => {
    // Arrange
    process.env.NODE_ENV = "development";
    process.env.AUTH_URL = "http://localhost:18321";

    // Act
    const result = shouldUseSecureAuthCookies();

    // Assert
    expect(result).toBe(false);
  });
});
