import { decodeJwt } from "jose";
import { redirect } from "next/navigation";
import { isRedirectError } from "next/dist/client/components/redirect-error";

export const LOGIN_PATH = "/login";

const API_ACCESS_TOKEN_CLOCK_SKEW_MS = 60_000;

export function isApiAccessTokenExpired(accessToken: string): boolean {
  try {
    const { exp } = decodeJwt(accessToken);
    if (!exp) {
      return false;
    }

    return exp * 1000 <= Date.now() + API_ACCESS_TOKEN_CLOCK_SKEW_MS;
  } catch {
    return true;
  }
}

export function signOutRedirectPath(): string {
  const callbackUrl = encodeURIComponent(LOGIN_PATH);
  return `/api/auth/signout?callbackUrl=${callbackUrl}`;
}

export function redirectToSignInAfterInvalidSession(): never {
  redirect(signOutRedirectPath());
}

export function rethrowIfNavigationError(error: unknown): void {
  if (isRedirectError(error)) {
    throw error;
  }
}
