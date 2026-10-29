"use server";

import { revalidatePath } from "next/cache";
import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export async function markAlertReadAction(alertId: string) {
  try {
    const api = await getServerApiClient();
    await api.postMemberAlertRead(alertId);
    revalidatePath("/alerts");
    revalidatePath("/", "layout");
    return { ok: true as const };
  } catch (error) {
    return { ok: false as const, message: actionErrorMessage(error, "Could not mark alert read") };
  }
}

export async function markAllAlertsReadAction() {
  try {
    const api = await getServerApiClient();
    await api.postMemberAlertsReadAll();
    revalidatePath("/alerts");
    revalidatePath("/", "layout");
    return { ok: true as const };
  } catch (error) {
    return { ok: false as const, message: actionErrorMessage(error, "Could not mark alerts read") };
  }
}
