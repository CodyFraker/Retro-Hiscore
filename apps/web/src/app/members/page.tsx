import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import { PageHero } from "@/components/layout/page-hero";
import { RivalryPicker } from "@/components/rivalry/rivalry-picker";
import type { MemberDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function MembersPage() {
  let members: MemberDto[] = [];
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    members = await api.getMembers();
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load members";
  }

  return (
    <div className="space-y-8">
      <PageHero
        title="Members"
        description="Tracked friends on Retro Hiscore. Open a profile for board leads and standings."
      />

      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          {error}
        </p>
      )}

      {!error && members.length === 0 && (
        <p className="text-muted-foreground">No members seeded yet.</p>
      )}

      {!error && members.length >= 2 && (
        <section id="head-to-head" className="space-y-3">
          <h2 className="steam-section-heading">Head-to-head</h2>
          <RivalryPicker members={members} />
        </section>
      )}

      <ul className="divide-y divide-border border-y border-border">
        {members.map((member) => {
          const rowClassName =
            "flex flex-col gap-2 py-5 transition-colors sm:flex-row sm:items-center sm:justify-between sm:gap-4";
          const rowContent = (
            <>
              <div className="flex items-center gap-3">
                <MemberAvatar avatarUrl={member.avatarUrl} displayName={member.displayName} size={40} />
                <div className="min-w-0">
                  <h2 className="truncate text-xl font-medium">{member.displayName}</h2>
                  {member.raUsername ? (
                    <p className="mt-1 font-mono text-xs text-muted-foreground">@{member.raUsername}</p>
                  ) : null}
                </div>
              </div>
              <div className="font-mono text-xs text-muted-foreground sm:text-right">
                <span className="text-foreground">{member.friendRankOnes}</span> lead
                {member.friendRankOnes === 1 ? "" : "s"}
                <span className="mx-2 text-border">·</span>
                {member.boardsWithScore} scored
              </div>
            </>
          );

          return (
            <li key={member.id}>
              {member.raUsername ? (
                <Link href={`/members/${encodeURIComponent(member.raUsername)}`} className={`${rowClassName} hover:bg-secondary/40`}>
                  {rowContent}
                </Link>
              ) : (
                <div className={rowClassName}>{rowContent}</div>
              )}
            </li>
          );
        })}
      </ul>
    </div>
  );
}
