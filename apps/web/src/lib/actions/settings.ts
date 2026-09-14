"use server";

import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type SettingsActionResult =
  | { ok: true }
  | { ok: false; error: string };

export async function saveMemberRaAccountAction(
  raUsername: string,
  apiKey: string,
): Promise<SettingsActionResult> {
  const trimmedUsername = raUsername.trim();
  const trimmedKey = apiKey.trim();
  if (!trimmedUsername || !trimmedKey) {
    return { ok: false, error: "Enter your RetroAchievements username and API key." };
  }

  try {
    const api = await getServerApiClient();
    await api.putMemberRaAccount(trimmedUsername, trimmedKey);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to save account") };
  }
}

export async function saveMemberApiKeyAction(apiKey: string): Promise<SettingsActionResult> {
  const trimmedKey = apiKey.trim();
  if (!trimmedKey) {
    return { ok: false, error: "Enter your RetroAchievements API key." };
  }

  try {
    const api = await getServerApiClient();
    await api.putMemberApiKey(trimmedKey);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to save API key") };
  }
}
