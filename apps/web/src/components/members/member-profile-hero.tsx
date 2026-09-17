import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { MemberRaStatsRow } from "@/components/members/member-ra-stats-row";
import { Badge } from "@/components/ui/badge";
import type { MemberRaSummaryResponse } from "@/generated/api-client";

type DetailProps = {
  displayName: string;
  avatarUrl?: string | null;
  raUsername: string;
  friendRankOnes: number;
  boardsWithScore: number;
};

type Props = {
  detail: DetailProps;
  raSummary: MemberRaSummaryResponse;
};

function statusBadgeVariant(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "online" || normalized.includes("playing")) {
    return "default" as const;
  }

  return "secondary" as const;
}

function formatMemberSince(value?: string | null) {
  if (!value) {
    return null;
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }

  return date.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

export function MemberProfileHero({ detail, raSummary }: Props) {
  const summary = raSummary.available ? raSummary.summary : null;
  const raStatus = summary?.status;
  const memberSince = summary ? formatMemberSince(summary.memberSince) : null;

  return (
    <div className="space-y-4">
      <Link
        href="/members"
        className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="size-4 shrink-0" />
        All members
      </Link>

      <div className="min-w-0 space-y-6">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-4">
            <MemberAvatar
              avatarUrl={detail.avatarUrl}
              displayName={detail.displayName}
              size={56}
            />
            <div className="flex min-w-0 flex-wrap items-center gap-3">
              <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)] sm:text-3xl">
                {detail.displayName}
              </h1>
              {raStatus ? (
                <Badge variant={statusBadgeVariant(raStatus)}>{raStatus}</Badge>
              ) : null}
            </div>
          </div>
          <p className="font-mono text-sm text-muted-foreground">@{detail.raUsername}</p>
          {summary?.motto ? (
            <p className="text-sm italic text-foreground">&ldquo;{summary.motto}&rdquo;</p>
          ) : null}
          {memberSince ? (
            <p className="text-sm text-muted-foreground">Member since {memberSince}</p>
          ) : null}
        </div>

        <MemberRaStatsRow
          friendRankOnes={detail.friendRankOnes}
          boardsWithScore={detail.boardsWithScore}
          summary={summary}
        />
      </div>
    </div>
  );
}
