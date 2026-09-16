import { canonicalRivalryUsernames } from "@/lib/rivalry-usernames";

export function rivalryPath(usernameA: string, usernameB: string): string {
  const [a, b] = canonicalRivalryUsernames(usernameA, usernameB);
  return `/rivalry/${encodeURIComponent(a)}/${encodeURIComponent(b)}`;
}
