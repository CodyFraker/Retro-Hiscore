import { SignJWT } from "jose";

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
    .setExpirationTime("24h")
    .sign(key);
}
