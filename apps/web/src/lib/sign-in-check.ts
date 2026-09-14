import { getApiBaseUrl } from "@/lib/api-base-url";

const SIGN_IN_SERVICE_KEY_HEADER = "X-SignIn-Service-Key";

export async function isDiscordUserInvited(discordUserId: string): Promise<boolean> {
  const baseUrl = getApiBaseUrl();
  const serviceKey = process.env.AUTH_SIGN_IN_SERVICE_KEY ?? process.env.AUTH_SECRET;
  if (!baseUrl || !serviceKey) {
    return false;
  }

  try {
    const params = new URLSearchParams({ discordId: discordUserId });
    const response = await fetch(`${baseUrl}/api/auth/sign-in-check?${params.toString()}`, {
      method: "GET",
      headers: {
        [SIGN_IN_SERVICE_KEY_HEADER]: serviceKey,
      },
      cache: "no-store",
    });

    return response.status === 204;
  } catch {
    return false;
  }
}
