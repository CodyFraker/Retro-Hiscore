import { getRedirectError, getURLFromRedirectError } from "next/dist/client/components/redirect";
import { isRedirectError } from "next/dist/client/components/redirect-error";

const mockDecodeJwt = jest.fn();

jest.mock("jose", () => ({
  decodeJwt: (...args: unknown[]) => mockDecodeJwt(...args),
}));

import {
  isApiAccessTokenExpired,
  redirectToSignInAfterInvalidSession,
  rethrowIfNavigationError,
  signOutRedirectPath,
} from "@/lib/invalid-session";

describe("signOutRedirectPath", () => {
  it("points at the silent session clear route", () => {
    // Arrange
    // Act
    const path = signOutRedirectPath();

    // Assert
    expect(path).toBe("/api/auth/clear-session");
  });
});

describe("isApiAccessTokenExpired", () => {
  beforeEach(() => {
    mockDecodeJwt.mockReset();
  });

  it("returns false when the token expires in the future", () => {
    // Arrange
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    mockDecodeJwt.mockReturnValue({ exp: futureExp });

    // Act
    const expired = isApiAccessTokenExpired("token");

    // Assert
    expect(expired).toBe(false);
  });

  it("returns true when the token is already expired", () => {
    // Arrange
    const pastExp = Math.floor(Date.now() / 1000) - 3600;
    mockDecodeJwt.mockReturnValue({ exp: pastExp });

    // Act
    const expired = isApiAccessTokenExpired("token");

    // Assert
    expect(expired).toBe(true);
  });

  it("returns false when the token has no expiration claim", () => {
    // Arrange
    mockDecodeJwt.mockReturnValue({ sub: "discord-id" });

    // Act
    const expired = isApiAccessTokenExpired("token");

    // Assert
    expect(expired).toBe(false);
  });

  it("returns true when the token cannot be decoded", () => {
    // Arrange
    mockDecodeJwt.mockImplementation(() => {
      throw new Error("invalid jwt");
    });

    // Act
    const expired = isApiAccessTokenExpired("not-a-jwt");

    // Assert
    expect(expired).toBe(true);
  });
});

describe("rethrowIfNavigationError", () => {
  it("rethrows Next.js redirect errors", () => {
    // Arrange
    const redirectError = getRedirectError(signOutRedirectPath(), "replace");

    // Act & Assert
    expect(() => rethrowIfNavigationError(redirectError)).toThrow(redirectError);
  });

  it("does not throw for ordinary errors", () => {
    // Arrange
    const error = new Error("API 500: boom");

    // Act & Assert
    expect(() => rethrowIfNavigationError(error)).not.toThrow();
  });
});

describe("redirectToSignInAfterInvalidSession", () => {
  it("throws a redirect to the sign-out route", () => {
    // Arrange
    const expectedPath = signOutRedirectPath();

    // Act & Assert
    expect(() => redirectToSignInAfterInvalidSession()).toThrow();
    try {
      redirectToSignInAfterInvalidSession();
    } catch (error) {
      expect(isRedirectError(error)).toBe(true);
      expect(getURLFromRedirectError(error)).toBe(expectedPath);
    }
  });
});
