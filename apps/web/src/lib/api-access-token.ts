import { SignJWT } from "jose";
import type { JWT } from "next-auth/jwt";
import { isApiAccessTokenExpired } from "@/lib/invalid-session";
import { SESSION_MAX_AGE_SECONDS } from "@/lib/session-duration";

export async function createApiAccessToken(
  discordUserId: string,
  discordUsername: string,
): Promise<string> {
  const secret = process.env.AUTH_SECRET;
  if (!secret) {
    throw new Error("AUTH_SECRET is not configured");
  }

  const key = new TextEncoder().encode(secret);

  return await new SignJWT({ name: discordUsername })
    .setProtectedHeader({ alg: "HS256" })
    .setSubject(discordUserId)
    .setJti(crypto.randomUUID())
    .setIssuedAt()
    .setExpirationTime(`${SESSION_MAX_AGE_SECONDS}s`)
    .sign(key);
}

export async function ensureApiAccessTokenOnJwt(token: JWT): Promise<JWT> {
  const discordId = token.discordId;
  const discordUsername = token.discordUsername;
  if (typeof discordId !== "string" || typeof discordUsername !== "string") {
    return token;
  }

  const existing = token.apiAccessToken;
  if (typeof existing === "string" && !isApiAccessTokenExpired(existing)) {
    return token;
  }

  token.apiAccessToken = await createApiAccessToken(discordId, discordUsername);
  return token;
}
