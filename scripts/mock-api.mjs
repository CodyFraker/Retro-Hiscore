import http from "node:http";

const members = [
  { id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", raUsername: "ShrimpPoboy", raUlid: null, displayName: "ShrimpPoboy" },
  { id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", raUsername: "beefboybilly", raUlid: null, displayName: "beefboybilly" },
  { id: "cccccccc-cccc-cccc-cccc-cccccccccccc", raUsername: "xXScubXx", raUlid: null, displayName: "xXScubXx" },
];

const games = [
  { id: "11111111-1111-1111-1111-111111111111", raGameId: 38130, title: "Game 38130", leaderboardCount: 1 },
  { id: "22222222-2222-2222-2222-222222222222", raGameId: 2291, title: "Game 2291", leaderboardCount: 0 },
  { id: "33333333-3333-3333-3333-333333333333", raGameId: 789, title: "Game 789", leaderboardCount: 0 },
];

const gameBoards = {
  gameId: "11111111-1111-1111-1111-111111111111",
  raGameId: 38130,
  title: "Game 38130",
  members: members.map(({ id, raUsername, displayName }) => ({ id, raUsername, displayName })),
  leaderboards: [
    {
      id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
      raLeaderboardId: 161476,
      title: "Space Cadet",
      description: "Get the highest score you can!",
      format: "VALUE",
      rankAsc: false,
      standings: [
        {
          memberId: members[0].id,
          raUsername: "ShrimpPoboy",
          displayName: "ShrimpPoboy",
          score: 352750,
          formattedScore: "352,750",
          globalRank: 259,
          friendRank: 1,
          scoreUpdatedAt: "2026-09-13T01:34:10+00:00",
        },
        {
          memberId: members[1].id,
          raUsername: "beefboybilly",
          displayName: "beefboybilly",
          score: null,
          formattedScore: null,
          globalRank: null,
          friendRank: null,
          scoreUpdatedAt: null,
        },
        {
          memberId: members[2].id,
          raUsername: "xXScubXx",
          displayName: "xXScubXx",
          score: null,
          formattedScore: null,
          globalRank: null,
          friendRank: null,
          scoreUpdatedAt: null,
        },
      ],
    },
  ],
};

const port = Number(process.env.MOCK_API_PORT || 18943);

const server = http.createServer((req, res) => {
  const url = new URL(req.url || "/", `http://localhost:${port}`);
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "*");
  if (req.method === "OPTIONS") {
    res.writeHead(204);
    res.end();
    return;
  }

  const json = (status, body) => {
    res.writeHead(status, { "Content-Type": "application/json" });
    res.end(JSON.stringify(body));
  };

  if (url.pathname === "/api/members") return json(200, members);
  if (url.pathname === "/api/games") return json(200, games);
  if (url.pathname === "/api/games/38130/leaderboards") return json(200, gameBoards);
  if (url.pathname === "/api/sync/status") {
    return json(200, { id: null, trigger: null, status: null, startedAt: null, finishedAt: null, error: null });
  }
  if (url.pathname === "/api/sync" && req.method === "POST") {
    return json(202, { jobId: "job-1" });
  }

  json(404, { message: "not found" });
});

server.listen(port, () => {
  console.log(`Mock API listening on http://localhost:${port}`);
});
