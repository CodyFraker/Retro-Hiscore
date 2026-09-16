export function defaultPayloadTemplateForEvent(kind: string): string {
  switch (kind) {
    case "GameTracked":
      return `{
  "embeds": [{
    "title": "{{gt}}",
    "description": "Now tracked on Retro-Hiscore!\\n{{nl}} leaderboards · {{na}} achievements · {{te}} total entries",
    "color": 5793266
  }]
}`;
    case "LeaderboardFriendOvertake":
      return `{
  "embeds": [{
    "title": "{{usr}}",
    "description": "Moved up **{{fd}}** on **{{gt}}** — {{lb}}\\nScore **{{sc}}** · friend #{{fr}}",
    "color": 3066993
  }]
}`;
    case "LeaderboardNewSubmission":
      return `{
  "embeds": [{
    "title": "{{usr}}",
    "description": "Posted on **{{gt}}** — {{lb}}\\n**{{sc}}** · friend #{{fr}}",
    "color": 3447003
  }]
}`;
    case "AchievementUnlocked":
      return `{
  "embeds": [{
    "title": "{{usr}}",
    "description": "Unlocked **{{ach}}** ({{ap}} pts) in **{{gt}}**",
    "color": 15844367
  }]
}`;
    default:
      return `{ "embeds": [{ "title": "Notification", "description": "Event" }] }`;
  }
}

export const DISCORD_EVENT_KIND_OPTIONS: { value: string; label: string }[] = [
  { value: "GameTracked", label: "Game tracked" },
  { value: "LeaderboardFriendOvertake", label: "Leaderboard overtake" },
  { value: "LeaderboardNewSubmission", label: "New leaderboard submission" },
  { value: "AchievementUnlocked", label: "Achievement unlocked" },
];
