"use server";

import type {
  AdminDiscordWebhookDetailDto,
  AdminGameDto,
  DiscordNotificationEventKind,
  GameOfTheWeekCurrentPollDto,
  GameSourceDto,
  PatchAdminSyncSettingsRequest,
  UpsertGameSourceRequest,
} from "@/generated/api-client";
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

export type PostAdminGameOfTheWeekPollResult =
  | { ok: true; poll: GameOfTheWeekCurrentPollDto }
  | { ok: false; error: string };

export async function postAdminGameOfTheWeekPollAction(
  startsAt: string,
  endsAt: string,
  raGameIds: number[],
): Promise<PostAdminGameOfTheWeekPollResult> {
  if (raGameIds.length < 2) {
    return { ok: false, error: "Provide at least two RetroAchievements game ids." };
  }

  try {
    const api = await getServerApiClient();
    const poll = await api.postAdminGameOfTheWeekPoll({
      startsAt,
      endsAt,
      raGameIds,
    });
    return { ok: true, poll };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to start poll") };
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

export type PatchAdminGameLeaderboardSyncResult =
  | { ok: true; game: AdminGameDto }
  | { ok: false; error: string };

export async function patchAdminGameLeaderboardSyncAction(
  raGameId: number,
  forceColdLeaderboardSync: boolean,
): Promise<PatchAdminGameLeaderboardSyncResult> {
  try {
    const api = await getServerApiClient();
    const game = await api.patchAdminGameLeaderboardSync(raGameId, forceColdLeaderboardSync);
    return { ok: true, game };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to update leaderboard sync") };
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

export type SaveAdminDiscordWebhookInput = {
  id: string | null;
  name: string;
  enabled: boolean;
  webhookUrl: string;
  digestIntervalMinutes: number;
  payloadTemplateJson: string;
  eventKinds: string[];
};

export async function getAdminDiscordWebhookAction(
  id: string,
): Promise<{ ok: true; webhook: AdminDiscordWebhookDetailDto } | { ok: false; error: string }> {
  try {
    const api = await getServerApiClient();
    const webhook = await api.getAdminDiscordWebhook(id);
    return { ok: true, webhook };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to load webhook") };
  }
}

export async function saveAdminDiscordWebhookAction(
  input: SaveAdminDiscordWebhookInput,
): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    const body = {
      name: input.name.trim(),
      enabled: input.enabled,
      webhookUrl: input.webhookUrl.trim(),
      digestIntervalMinutes: input.digestIntervalMinutes,
      payloadTemplateJson: input.payloadTemplateJson,
      eventKinds: input.eventKinds,
      allowedRaGameIds: null,
    };
    if (input.id) {
      await api.putAdminDiscordWebhook(input.id, body);
    } else {
      await api.postAdminDiscordWebhook(body);
    }
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to save webhook") };
  }
}

export async function deleteAdminDiscordWebhookAction(id: string): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.deleteAdminDiscordWebhook(id);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to delete webhook") };
  }
}

export async function postAdminDiscordWebhookTestAction(
  id: string,
  eventKind: string,
): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.postAdminDiscordWebhookTest(id, { eventKind });
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Test webhook failed") };
  }
}

export async function patchAdminSyncSettingsAction(
  body: PatchAdminSyncSettingsRequest,
): Promise<AdminActionResult> {
  try {
    const api = await getServerApiClient();
    await api.patchAdminSyncSettings(body);
    return { ok: true };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Failed to save sync schedules") };
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
