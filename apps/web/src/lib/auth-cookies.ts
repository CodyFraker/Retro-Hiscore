export function shouldUseSecureAuthCookies(): boolean {
  if (process.env.NODE_ENV !== "production") {
    return false;
  }

  const authUrl = process.env.AUTH_URL ?? process.env.NEXTAUTH_URL ?? "";
  return authUrl.startsWith("https://");
}
