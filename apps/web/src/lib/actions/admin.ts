"use server";

import type { AdminGameDto, GameSourceDto, UpsertGameSourceRequest } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage, formatAddGameError } from "@/lib/action-error";

export type AdminActionResult =
  | { ok: true }
  | { ok: false; error: string };

export type AddAdminGameResult =
  | { ok: true; game: AdminGameDto }
  | { ok: false; error: string };

export type UpsertGameSourceResult =
  | { ok: true; source: GameSourceDto }
  | { ok: false; error: string };

export async function postAdminMemberInviteAction(discordId: string): Promise<AdminActionResult> {
  const trimmed = discordId.trim();
  if (!trimmed) {
    return { ok: false, error: "Enter a Discord user ID." };
  }

  try {
    const api = await getServerApiClient();
    await api.postAdminMemberInvite(trimmed);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to add invite") };
  }
}

export async function deleteAdminMemberInviteAction(discordId: string): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.deleteAdminMemberInvite(discordId);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to remove invite") };
  }
}

export async function postAdminGameAction(raGameId: number): Promise<AddAdminGameResult> {
  try {
    const api = await getServerApiClient();
    const game = await api.postAdminGame(raGameId);
    return { ok: true, game };
  } catch (error) {
    return { ok: false, error: formatAddGameError(error) };
  }
}

export async function approveAdminGameTrackQueueItemAction(id: string): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.postAdminGameTrackQueueApprove(id);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Approve failed") };
  }
}

export async function rejectAdminGameTrackQueueItemAction(id: string): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.postAdminGameTrackQueueReject(id);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Reject failed") };
  }
}

export async function postAdminGameRefreshAction(raGameId: number): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.postAdminGameRefresh(raGameId);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Refresh failed") };
  }
}

export async function deleteAdminGameAction(raGameId: number): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.deleteAdminGame(raGameId);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Delete failed") };
  }
}

export async function deleteAdminGameSourceAction(
  raGameId: number,
  sourceId: string,
): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.deleteAdminGameSource(raGameId, sourceId);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Delete failed") };
  }
}

export async function upsertAdminGameSourceAction(
  raGameId: number,
  editingId: string | null,
  form: UpsertGameSourceRequest,
): Promise<UpsertGameSourceResult> {
  const payload: UpsertGameSourceRequest = {
    ...form,
    label: form.label?.trim() || null,
    note: form.note?.trim() || null,
  };

  try {
    const api = await getServerApiClient();
    const source = editingId
      ? await api.putAdminGameSource(raGameId, editingId, payload)
      : await api.postAdminGameSource(raGameId, payload);
    return { ok: true, source };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Save failed") };
  }
}
