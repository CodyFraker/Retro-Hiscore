import { pathnameToRouteTemplate } from "@/lib/telemetry/route-template";

describe("pathnameToRouteTemplate", () => {
  it("returns static routes unchanged", () => {
    expect(pathnameToRouteTemplate("/games")).toBe("/games");
    expect(pathnameToRouteTemplate("/admin/sync")).toBe("/admin/sync");
  });

  it("normalizes game detail paths", () => {
    expect(pathnameToRouteTemplate("/games/12345")).toBe("/games/:raGameId");
  });

  it("normalizes member profile paths", () => {
    expect(pathnameToRouteTemplate("/members/someone")).toBe("/members/:raUsername");
  });

  it("normalizes leaderboard paths", () => {
    expect(pathnameToRouteTemplate("/leaderboards/999")).toBe("/leaderboards/:raLeaderboardId");
  });

  it("normalizes admin game paths", () => {
    expect(pathnameToRouteTemplate("/admin/games/42")).toBe("/admin/games/:raGameId");
  });

  it("normalizes rivalry paths", () => {
    expect(pathnameToRouteTemplate("/rivalry/alice/bob")).toBe("/rivalry/:usernameA/:usernameB");
  });
});
