import { NextRequest, NextResponse } from "next/server";
import { applyClearedSessionCookies } from "@/lib/clear-next-auth-session";
import { LOGIN_PATH } from "@/lib/invalid-session";

export async function GET(request: NextRequest) {
  const response = NextResponse.redirect(new URL(LOGIN_PATH, request.url));
  applyClearedSessionCookies(request, response);
  return response;
}
