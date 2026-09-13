import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { MemberStandingsSection } from "@/components/members/member-standings-section";
import { MemberTrendCharts } from "@/components/members/member-trend-charts";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raUsername: string }>;
};

export default async function MemberProfilePage({ params }: Props) {
  const { raUsername: raw } = await params;
  const raUsername = decodeURIComponent(raw);

  const api = await getServerApiClient();
  let detail: Awaited<ReturnType<typeof api.getMember>>;
  let history: Awaited<ReturnType<typeof api.getMemberHistory>>;
  try {
    [detail, history] = await Promise.all([
      api.getMember(raUsername),
      api.getMemberHistory(raUsername, 200, 0),
    ]);
  } catch {
    notFound();
  }

  return (
    <div className="space-y-10">
      <div className="space-y-2">
        <Link
          href="/members"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="size-4 shrink-0" />
          All members
        </Link>
        <h1 className="font-[family-name:var(--font-display)] text-3xl text-[var(--accent-retro)]">
          {detail.displayName}
        </h1>
        <p className="font-mono text-sm text-muted-foreground">@{detail.raUsername}</p>
        <p className="text-sm text-muted-foreground">
          Leading <span className="font-mono text-foreground">{detail.friendRankOnes}</span> board
          {detail.friendRankOnes === 1 ? "" : "s"}
          <span className="mx-2 text-border">·</span>
          Scored on <span className="font-mono text-foreground">{detail.boardsWithScore}</span>
        </p>
      </div>

      <MemberTrendCharts items={history.items} />

      <section className="space-y-3">
        <h2 className="text-lg font-medium">Current standings</h2>
        {detail.standings.length === 0 ? (
          <p className="text-muted-foreground">No scores synced yet.</p>
        ) : (
          <div className="rounded border border-border p-3 md:p-0 md:overflow-x-auto">
            <MemberStandingsSection standings={detail.standings} />
          </div>
        )}
      </section>
    </div>
  );
}
