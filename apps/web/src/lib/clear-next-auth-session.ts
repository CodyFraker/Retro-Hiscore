import type { NextRequest, NextResponse } from "next/server";

export function useSecureAuthCookies(): boolean {
  const authUrl = process.env.AUTH_URL ?? process.env.NEXTAUTH_URL ?? "";
  if (authUrl.startsWith("https://")) {
    return true;
  }

  return process.env.NODE_ENV === "production";
}

export function sessionCookiePrefix(): string {
  const prefix = useSecureAuthCookies() ? "__Secure-" : "";
  return `${prefix}next-auth.session-token`;
}

export function applyClearedSessionCookies(request: NextRequest, response: NextResponse): void {
  const prefix = sessionCookiePrefix();
  const secure = useSecureAuthCookies();

  for (const { name } of request.cookies.getAll()) {
    if (!name.includes("next-auth") && !name.startsWith(prefix)) {
      continue;
    }

    response.cookies.set(name, "", {
      httpOnly: true,
      sameSite: "lax",
      path: "/",
      secure,
      maxAge: 0,
    });
  }
}
