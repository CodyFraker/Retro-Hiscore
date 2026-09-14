import { getApiBaseUrl } from "@/lib/api-base-url";

export async function syncMemberProfileOnLogin(
  accessToken: string,
  avatarUrl: string | null | undefined,
): Promise<void> {
  const baseUrl = getApiBaseUrl();
  if (!baseUrl) {
    return;
  }

  try {
    await fetch(`${baseUrl}/api/members/me/profile`, {
      method: "PUT",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({ avatarUrl: avatarUrl ?? null }),
    });
  } catch {
    // Profile sync is best-effort on login.
  }
}
