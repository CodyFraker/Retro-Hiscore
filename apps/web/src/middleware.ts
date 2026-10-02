import { withAuth } from "next-auth/middleware";
import { NextResponse } from "next/server";
import { isApiAccessTokenExpired, signOutRedirectPath } from "@/lib/invalid-session";

export default withAuth(
  function middleware(req) {
    const token = req.nextauth.token;
    const path = req.nextUrl.pathname;

    const apiAccessToken = token?.apiAccessToken;
    if (typeof apiAccessToken === "string" && isApiAccessTokenExpired(apiAccessToken)) {
      return NextResponse.redirect(new URL(signOutRedirectPath(), req.url));
    }

    if (
      token?.needsOnboarding === true &&
      path !== "/settings" &&
      !path.startsWith("/api/auth")
    ) {
      return NextResponse.redirect(new URL("/settings", req.url));
    }

    const requestHeaders = new Headers(req.headers);
    requestHeaders.set("x-pathname", path);
    return NextResponse.next({
      request: { headers: requestHeaders },
    });
  },
  {
    pages: {
      signIn: "/login",
    },
  },
);

export const config = {
  matcher: ["/((?!login|api/auth|_next/static|_next/image|favicon.ico).*)"],
};
