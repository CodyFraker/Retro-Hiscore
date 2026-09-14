"use server";

import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type GameSyncActionResult =
  | { ok: true; message: string }
  | { ok: false; message: string };

export async function triggerGameRefreshAction(raGameId: number): Promise<GameSyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.postGameRefresh(raGameId);
    if (!result.ok) {
      return { ok: false, message: result.body.message };
    }
    return { ok: true, message: "Refresh queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Refresh failed") };
  }
}
