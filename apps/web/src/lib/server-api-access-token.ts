import { cookies } from "next/headers";
import { getToken } from "next-auth/jwt";
import { isApiAccessTokenExpired } from "@/lib/invalid-session";

function resolveAuthSecret(): string | undefined {
  return process.env.AUTH_SECRET ?? process.env.NEXTAUTH_SECRET;
}

export async function getServerApiAccessToken(): Promise<string | undefined> {
  const secret = resolveAuthSecret();
  if (!secret) {
    return undefined;
  }

  const cookieStore = await cookies();
  const jwt = await getToken({
    req: { headers: { cookie: cookieStore.toString() } },
    secret,
  });

  const accessToken = jwt?.apiAccessToken;
  if (typeof accessToken !== "string" || isApiAccessTokenExpired(accessToken)) {
    return undefined;
  }

  return accessToken;
}

export async function hasValidServerApiAccess(): Promise<boolean> {
  const token = await getServerApiAccessToken();
  return typeof token === "string";
}
