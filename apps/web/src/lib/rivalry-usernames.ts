export function canonicalRivalryUsernames(usernameA: string, usernameB: string): [string, string] {
  const a = usernameA.trim();
  const b = usernameB.trim();
  return a.localeCompare(b, undefined, { sensitivity: "accent" }) <= 0 ? [a, b] : [b, a];
}
