"use server";

import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type SyncActionResult =
  | { ok: true; message: string }
  | { ok: false; message: string };

function cooldownMessage(result: { ok: false; body: { message: string } }): string {
  return result.body.message;
}

export async function triggerScoreSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerSync();
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
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
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: "Metadata sync queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Metadata sync failed") };
  }
}

export async function triggerDueDispatchSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerDueDispatchSync();
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: "Due games sync queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Due games sync failed") };
  }
}

export async function triggerConsoleIconSyncAction(force = false): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerConsoleIconSync(force);
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: force ? "Console icon sync queued (force)" : "Console icon sync queued" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Console icon sync failed") };
  }
}

export async function triggerMemberActivitySyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerMemberActivitySync();
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: "Member activity sync started" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Member activity sync failed") };
  }
}

export async function triggerMemberRankSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerMemberRankSync();
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: "Member rank sync started" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Member rank sync failed") };
  }
}

export async function triggerMemberAchievementSyncAction(): Promise<SyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.triggerMemberAchievementSync();
    if (!result.ok) {
      return { ok: false, message: cooldownMessage(result) };
    }
    return { ok: true, message: "Member achievement sync started" };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Member achievement sync failed") };
  }
}
