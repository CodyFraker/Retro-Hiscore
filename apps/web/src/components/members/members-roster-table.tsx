import { Crown } from "lucide-react";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { MemberAvatar } from "@/components/members/member-avatar";
import { MemberRaGameLink } from "@/components/members/member-ra-game-link";
import { RetroachievementsUserLink } from "@/components/members/retroachievements-user-link";
import type { MemberDto } from "@/generated/api-client";
import {
  formatRaTrend,
  formatRankLabel,
  shouldShowCompactPresence,
} from "@/lib/ra-member-metrics";

type RosterRow = MemberDto & { rosterIndex: number };

type Props = {
  members: MemberDto[];
};

function memberProfileHref(raUsername?: string | null) {
  if (!raUsername) {
    return null;
  }

  return `/members/${encodeURIComponent(raUsername)}`;
}

function FriendStats({ member }: { member: MemberDto }) {
  return (
    <span className="font-mono text-xs text-muted-foreground">
      <span className="text-foreground">{member.friendRankOnes}</span> lead
      {member.friendRankOnes === 1 ? "" : "s"}
      <span className="mx-2 text-border">·</span>
      {member.boardsWithScore} scored
    </span>
  );
}

function PointsCell({ member }: { member: MemberDto }) {
  const showSoftcore = (member.raTotalSoftcorePoints ?? 0) > 0;

  return (
    <div className="space-y-0.5 font-mono text-xs tabular-nums">
      <div>
        <span className="text-muted-foreground">HC </span>
        <span>{member.raTotalPoints?.toLocaleString() ?? "—"}</span>
      </div>
      {showSoftcore ? (
        <div>
          <span className="text-muted-foreground">SC </span>
          <span>{member.raTotalSoftcorePoints!.toLocaleString()}</span>
        </div>
      ) : null}
    </div>
  );
}

function PresenceCell({ member }: { member: MemberDto }) {
  if (!shouldShowCompactPresence(member.raStatus, member.raPresenceGameTitle)) {
    return <span className="text-muted-foreground">—</span>;
  }

  const gameId = member.raPresenceRaGameId;
  if (gameId == null) {
    return <span className="truncate text-sm">{member.raPresenceGameTitle}</span>;
  }

  return (
    <div className="flex min-w-0 max-w-[12rem] items-center gap-2">
      <span
        className="size-2 shrink-0 rounded-full bg-[var(--accent-retro)]"
        title={member.raStatus ?? undefined}
      />
      <MemberRaGameLink
        raGameId={gameId}
        isTracked={member.raPresenceIsTracked ?? false}
        className="truncate text-sm hover:text-[var(--accent-retro)]"
      >
        {member.raPresenceGameTitle}
      </MemberRaGameLink>
    </div>
  );
}

function MemberIdentity({ row }: { row: RosterRow }) {
  const href = memberProfileHref(row.raUsername);
  const showCrown = row.rosterIndex === 0 && row.friendRankOnes > 0;

  const content = (
    <div className="flex min-w-0 items-center gap-3">
      {showCrown ? <Crown className="size-4 shrink-0 text-[var(--accent-retro)]" /> : null}
      <MemberAvatar avatarUrl={row.avatarUrl} displayName={row.displayName} size={36} />
      <div className="min-w-0">
        <p className="truncate font-medium">{row.displayName}</p>
        {row.raUsername ? (
          <p className="truncate font-mono text-xs text-muted-foreground">@{row.raUsername}</p>
        ) : null}
      </div>
    </div>
  );

  if (!href) {
    return content;
  }

  return (
    <Link href={href} className="block min-w-0 hover:text-[var(--accent-retro)]">
      {content}
    </Link>
  );
}

export function MembersRosterTable({ members }: Props) {
  const rows: RosterRow[] = members.map((member, rosterIndex) => ({
    ...member,
    rosterIndex,
  }));

  const trendFor = (row: RosterRow) => formatRaTrend(row.raRankDelta, row.raPointsDelta);

  return (
    <ResponsiveTable
      rows={rows}
      rowKey={(row) => row.id}
      columns={[
        {
          header: "Member",
          mobileProminent: true,
          render: (row) => <MemberIdentity row={row} />,
        },
        {
          header: "Friend stats",
          mobileLabel: "Friend stats",
          render: (row) => <FriendStats member={row} />,
        },
        {
          header: "RA rank",
          mobileLabel: "RA rank",
          render: (row) => (
            <div className="space-y-0.5">
              <p className="font-mono text-sm tabular-nums">
                {formatRankLabel(row.raRank, row.raTotalRanked)}
              </p>
              {trendFor(row) ? (
                <p className="text-xs text-[var(--accent-retro)]">{trendFor(row)}</p>
              ) : null}
            </div>
          ),
        },
        {
          header: "Points",
          mobileLabel: "Points",
          render: (row) => <PointsCell member={row} />,
        },
        {
          header: "Last active",
          mobileLabel: "Last active",
          render: (row) =>
            row.lastActiveAt ? (
              <FormattedSyncTime value={row.lastActiveAt} />
            ) : (
              <span className="text-muted-foreground">—</span>
            ),
        },
        {
          header: "Now playing",
          mobileLabel: "Now playing",
          render: (row) => <PresenceCell member={row} />,
        },
        {
          header: "",
          headerClassName: "w-12",
          mobileHidden: true,
          render: (row) => (
            <RetroachievementsUserLink
              raUsername={row.raUsername}
              raUlid={row.raUlid}
              iconOnly
            />
          ),
        },
      ]}
      renderMobileCard={(row) => (
        <div className="space-y-3 rounded border border-border bg-card p-4 text-sm">
          <MemberIdentity row={row} />
          <dl className="grid gap-2 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">Friend stats</dt>
              <dd><FriendStats member={row} /></dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">RA rank</dt>
              <dd className="text-right font-mono tabular-nums">
                {formatRankLabel(row.raRank, row.raTotalRanked)}
                {trendFor(row) ? (
                  <span className="mt-0.5 block text-xs text-[var(--accent-retro)]">
                    {trendFor(row)}
                  </span>
                ) : null}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">Points</dt>
              <dd><PointsCell member={row} /></dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">Last active</dt>
              <dd>
                {row.lastActiveAt ? (
                  <FormattedSyncTime value={row.lastActiveAt} />
                ) : (
                  "—"
                )}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">Now playing</dt>
              <dd className="min-w-0 text-right"><PresenceCell member={row} /></dd>
            </div>
          </dl>
          <RetroachievementsUserLink raUsername={row.raUsername} raUlid={row.raUlid} />
        </div>
      )}
    />
  );
}
