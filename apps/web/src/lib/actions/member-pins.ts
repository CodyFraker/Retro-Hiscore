"use server";

import { revalidatePath } from "next/cache";
import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export async function pinGameAction(raGameId: number) {
  try {
    const api = await getServerApiClient();
    await api.putMemberPin(raGameId);
    revalidatePath("/");
    revalidatePath("/games");
    revalidatePath(`/games/${raGameId}`);
    return { ok: true as const };
  } catch (error) {
    return { ok: false as const, message: actionErrorMessage(error, "Could not pin game") };
  }
}

export async function unpinGameAction(raGameId: number) {
  try {
    const api = await getServerApiClient();
    await api.deleteMemberPin(raGameId);
    revalidatePath("/");
    revalidatePath("/games");
    revalidatePath(`/games/${raGameId}`);
    return { ok: true as const };
  } catch (error) {
    return { ok: false as const, message: actionErrorMessage(error, "Could not unpin game") };
  }
}
