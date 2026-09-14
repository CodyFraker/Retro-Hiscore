export function actionErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof Error)) {
    return fallback;
  }

  const match = error.message.match(/^API (\d+):\s*([\s\S]*)$/);
  if (!match) {
    return error.message;
  }

  const status = Number(match[1]);
  const body = match[2];
  const messageMatch = body.match(/"message"\s*:\s*"([^"]+)"/);
  if (messageMatch?.[1]) {
    return messageMatch[1];
  }

  if (status === 409) {
    return "That resource already exists.";
  }
  if (status === 404) {
    return "Not found.";
  }

  return error.message;
}

export function formatAddGameError(error: unknown): string {
  const message = actionErrorMessage(error, "Failed to add game");
  if (message === "That resource already exists.") {
    return "That game is already tracked.";
  }
  if (message === "Not found.") {
    return "Game not found on RetroAchievements.";
  }
  return message;
}
