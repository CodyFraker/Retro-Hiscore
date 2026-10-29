"use server";

import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";
import type { SeasonStandingsResponse } from "@/generated/api-client";

export async function loadSeasonStandingsAction(
  year: number,
  month: number,
): Promise<{ ok: true; data: SeasonStandingsResponse } | { ok: false; message: string }> {
  try {
    const api = await getServerApiClient();
    const data = await api.getDashboardSeason(year, month);
    return { ok: true, data };
  } catch (error) {
    return { ok: false, message: actionErrorMessage(error, "Failed to load season standings") };
  }
}
