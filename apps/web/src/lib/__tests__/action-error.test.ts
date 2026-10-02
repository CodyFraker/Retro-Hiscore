import { getRedirectError } from "next/dist/client/components/redirect";

jest.mock("jose", () => ({
  decodeJwt: jest.fn(),
}));

import { actionErrorMessage } from "@/lib/action-error";
import { signOutRedirectPath } from "@/lib/invalid-session";

describe("actionErrorMessage", () => {
  it("rethrows Next.js redirect errors instead of returning a message", () => {
    // Arrange
    const redirectError = getRedirectError(signOutRedirectPath(), "replace");

    // Act & Assert
    expect(() => actionErrorMessage(redirectError, "fallback")).toThrow(redirectError);
  });

  it("returns a parsed API error message for non-redirect failures", () => {
    // Arrange
    const error = new Error('API 404: {"message":"Not found."}');

    // Act
    const message = actionErrorMessage(error, "fallback");

    // Assert
    expect(message).toBe("Not found.");
  });
});
