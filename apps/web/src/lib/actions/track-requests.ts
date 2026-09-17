"use server";

import type { GameTrackRequestQuotaDto, GameTrackRequestSubmitResponse } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type PostGameTrackRequestResult =
  | { ok: true; response: GameTrackRequestSubmitResponse; status: number }
  | { ok: false; error: string };

export async function postGameTrackRequestAction(raGameId: number): Promise<PostGameTrackRequestResult> {
  try {
    const api = await getServerApiClient();
    const response = await api.postGameTrackRequest(raGameId);
    return { ok: true, response, status: 201 };
  } catch (error) {
    const parsed = parseTrackRequestError(error);
    if (parsed) {
      return { ok: true, response: parsed.body, status: parsed.status };
    }
    return { ok: false, error: actionErrorMessage(error, "Failed to submit track request") };
  }
}

export async function getGameTrackRequestQuotaAction(): Promise<GameTrackRequestQuotaDto | null> {
  try {
    const api = await getServerApiClient();
    return await api.getGameTrackRequestQuota();
  } catch {
    return null;
  }
}

function parseTrackRequestError(
  error: unknown,
): { status: number; body: GameTrackRequestSubmitResponse } | null {
  if (!(error instanceof Error)) {
    return null;
  }

  const match = error.message.match(/^API (\d+):([\s\S]*)$/);
  if (!match?.[1] || !match[2]) {
    return null;
  }

  const status = Number.parseInt(match[1], 10);
  if (!Number.isFinite(status)) {
    return null;
  }

  if (status !== 200 && status !== 409 && status !== 429) {
    return null;
  }

  try {
    const body = JSON.parse(match[2]) as GameTrackRequestSubmitResponse;
    if (!body?.code) {
      return null;
    }
    return { status, body };
  } catch {
    return null;
  }
}
