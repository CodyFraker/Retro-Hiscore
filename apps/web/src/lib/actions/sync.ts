"use server";

import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type SyncActionResult =
  | { ok: true; message: string }
  | { ok: false; message: string };

export async function triggerScoreSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerSync();
    if (!result.ok) {
      return { ok: false, message: result.body.message };
    }
    return { ok: true, message: "Sync queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Sync failed") };
  }
}

export async function triggerMetadataSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerMetadataSync();
    if (!result.ok) {
      return { ok: false, message: result.body.message };
    }
    return { ok: true, message: "Metadata sync queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Metadata sync failed") };
  }
}
