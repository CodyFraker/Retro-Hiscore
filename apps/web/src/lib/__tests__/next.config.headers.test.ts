import { buildSecurityHeaders } from "@/lib/security-headers";

describe("buildSecurityHeaders", () => {
  const originalNodeEnv = process.env.NODE_ENV;

  afterEach(() => {
    process.env.NODE_ENV = originalNodeEnv;
  });

  it("returns security headers in production", () => {
    // Arrange
    process.env.NODE_ENV = "production";

    // Act
    const headers = buildSecurityHeaders();

    // Assert
    expect(headers[0]?.headers?.some((h) => h.key === "X-Frame-Options")).toBe(true);
    expect(headers[0]?.headers?.some((h) => h.key === "X-Content-Type-Options")).toBe(true);
  });

  it("returns no headers outside production", () => {
    // Arrange
    process.env.NODE_ENV = "development";

    // Act
    const headers = buildSecurityHeaders();

    // Assert
    expect(headers).toEqual([]);
  });
});
