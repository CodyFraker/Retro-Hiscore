const STATIC_ROUTES = new Set([
  "/",
  "/login",
  "/settings",
  "/games",
  "/members",
  "/activity",
  "/achievements",
  "/rivalry",
  "/game-of-the-week",
  "/admin",
  "/admin/sync",
  "/admin/games",
  "/admin/members",
  "/admin/webhooks",
  "/admin/game-of-the-week",
  "/admin/theme-lab",
]);

export function pathnameToRouteTemplate(pathname: string): string {
  const path = pathname.split("?")[0].replace(/\/$/, "") || "/";
  if (STATIC_ROUTES.has(path)) {
    return path;
  }

  const segments = path.split("/").filter(Boolean);
  if (segments.length === 0) {
    return "/";
  }

  if (segments[0] === "games" && segments.length === 2 && /^\d+$/.test(segments[1])) {
    return "/games/:raGameId";
  }

  if (segments[0] === "members" && segments.length === 2) {
    return "/members/:raUsername";
  }

  if (segments[0] === "leaderboards" && segments.length === 2 && /^\d+$/.test(segments[1])) {
    return "/leaderboards/:raLeaderboardId";
  }

  if (segments[0] === "admin" && segments[1] === "games" && segments.length === 3 && /^\d+$/.test(segments[2])) {
    return "/admin/games/:raGameId";
  }

  if (segments[0] === "rivalry" && segments.length === 3) {
    return "/rivalry/:usernameA/:usernameB";
  }

  if (segments[0] === "rivalry" && segments.length === 1) {
    return "/rivalry";
  }

  return `/${segments.map((segment) => (/^\d+$/.test(segment) ? ":id" : segment)).join("/")}`;
}
