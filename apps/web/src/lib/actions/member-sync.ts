"use server";

import { getServerApiClient } from "@/lib/api";
import { formatSyncTime } from "@/lib/format";

export type MemberSyncActionResult = { ok: true; message: string } | { ok: false; message: string };

function actionErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error) {
    const match = error.message.match(/API 429: (.+)/);
    if (match) {
      try {
        const payload = JSON.parse(match[1]) as { availableAt?: string; message?: string };
        if (payload.availableAt) {
          return `${payload.message ?? "Cooldown active"} until ${formatSyncTime(payload.availableAt)}`;
        }
      } catch {
        return match[1];
      }
    }
    if (error.message.startsWith("API 403:")) {
      try {
        const payload = JSON.parse(error.message.replace(/^API 403:\s*/, "")) as { message?: string };
        return payload.message ?? fallback;
      } catch {
        return fallback;
      }
    }
    return error.message;
  }
  return fallback;
}

export async function triggerMemberSelfSyncLeaderboardsAction(): Promise<MemberSyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.postMemberSelfSyncLeaderboards();
    const skipped =
      result.skippedCooldown > 0 ? ` (${result.skippedCooldown} skipped — per-game cooldown)` : "";
    return {
      ok: true,
      message: `Queued leaderboard refresh for ${result.queued} game${result.queued === 1 ? "" : "s"}${skipped}`,
    };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Leaderboard sync failed") };
  }
}

export async function triggerMemberSelfSyncProfileAction(): Promise<MemberSyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.postMemberSelfSyncProfile();
    return {
      ok: true,
      message: result.status === "Succeeded" ? "Profile synced" : `Profile sync finished (${result.status})`,
    };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Profile sync failed") };
  }
}

export async function triggerMemberSelfSyncAchievementsAction(): Promise<MemberSyncActionResult> {
  try {
    const api = await getServerApiClient();
    const result = await api.postMemberSelfSyncAchievements();
    return {
      ok: true,
      message:
        result.status === "Succeeded" ? "Achievements synced" : `Achievement sync finished (${result.status})`,
    };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Achievement sync failed") };
  }
}

export async function triggerMemberSelfSyncAllAction(): Promise<MemberSyncActionResult> {
  const steps: MemberSyncActionResult[] = [];
  steps.push(await triggerMemberSelfSyncLeaderboardsAction());
  steps.push(await triggerMemberSelfSyncProfileAction());
  steps.push(await triggerMemberSelfSyncAchievementsAction());
  const failed = steps.filter((s) => !s.ok);
  if (failed.length === 0) {
    return { ok: true, message: steps.map((s) => s.message).join(" · ") };
  }
  return {
    ok: false,
    message: failed.map((s) => s.message).join(" · "),
  };
}
