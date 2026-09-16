"use server";

import type { GameOfTheWeekCurrentPollDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";
import { actionErrorMessage } from "@/lib/action-error";

export type GameOfTheWeekActionResult =
  | { ok: true; poll: GameOfTheWeekCurrentPollDto }
  | { ok: false; error: string };

export async function postGameOfTheWeekBallotAction(raGameId: number): Promise<GameOfTheWeekActionResult> {
  try {
    const api = await getServerApiClient();
    const poll = await api.postGameOfTheWeekBallot(raGameId);
    return { ok: true, poll };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Could not add game to ballot") };
  }
}

export async function putGameOfTheWeekVoteAction(raGameId: number): Promise<GameOfTheWeekActionResult> {
  try {
    const api = await getServerApiClient();
    const poll = await api.putGameOfTheWeekVote(raGameId);
    return { ok: true, poll };
  } catch (error) {
    return { ok: false, error: actionErrorMessage(error, "Could not save vote") };
  }
}
