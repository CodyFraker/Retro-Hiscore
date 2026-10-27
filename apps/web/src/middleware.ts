import { withAuth } from "next-auth/middleware";
import { NextResponse } from "next/server";

export default withAuth(
  function middleware(req) {
    const token = req.nextauth.token;
    const path = req.nextUrl.pathname;

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
