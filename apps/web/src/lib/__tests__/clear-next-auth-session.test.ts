import { NextRequest, NextResponse } from "next/server";
import {
  applyClearedSessionCookies,
  sessionCookiePrefix,
  useSecureAuthCookies,
} from "@/lib/clear-next-auth-session";

describe("useSecureAuthCookies", () => {
  const originalEnv = process.env;

  beforeEach(() => {
    process.env = { ...originalEnv };
  });

  afterAll(() => {
    process.env = originalEnv;
  });

  it("returns true when AUTH_URL is https", () => {
    // Arrange
    process.env.AUTH_URL = "https://retro.example.com";
    delete process.env.NEXTAUTH_URL;
    process.env.NODE_ENV = "development";

    // Act
    const secure = useSecureAuthCookies();

    // Assert
    expect(secure).toBe(true);
  });
});

describe("applyClearedSessionCookies", () => {
  it("clears chunked session cookies and redirects response can be built", () => {
    // Arrange
    const prefix = sessionCookiePrefix();
    const request = new NextRequest("http://localhost:18321/", {
      headers: {
        cookie: `${prefix}=token; ${prefix}.1=part2; next-auth.csrf-token=csrf; other=value`,
      },
    });
    const response = NextResponse.redirect("http://localhost:18321/login");

    // Act
    applyClearedSessionCookies(request, response);

    // Assert
    const cleared = response.cookies
      .getAll()
      .filter((cookie) => cookie.name.includes("next-auth") || cookie.name.startsWith(prefix));
    expect(cleared).toHaveLength(3);
    expect(cleared.every((cookie) => cookie.value === "" && cookie.maxAge === 0)).toBe(true);
  });
});
