import Link from "next/link";
import { RivalryPicker } from "@/components/rivalry/rivalry-picker";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function MembersPage() {
  let members: Awaited<ReturnType<ReturnType<typeof getServerApiClient>["getMembers"]>> = [];
  let error: string | null = null;

  try {
    members = await (await getServerApiClient()).getMembers();
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load members";
  }

  return (
    <div className="space-y-8">
      <section className="space-y-2">
        <h1 className="font-[family-name:var(--font-display)] text-4xl tracking-tight text-[var(--accent-retro)]">
          Members
        </h1>
        <p className="max-w-2xl text-muted-foreground">
          Tracked friends on Retro Hiscore. Open a profile for board leads and standings.
        </p>
      </section>

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
          <h2 className="text-lg font-medium">Head-to-head</h2>
          <RivalryPicker members={members} />
        </section>
      )}

      <ul className="divide-y divide-border border-y border-border">
        {members.map((member) => (
          <li key={member.id}>
            <Link
              href={`/members/${encodeURIComponent(member.raUsername)}`}
              className="flex flex-col gap-2 py-5 transition-colors hover:bg-secondary/40 sm:flex-row sm:items-center sm:justify-between sm:gap-4"
            >
              <div>
                <h2 className="text-xl font-medium">{member.displayName}</h2>
                <p className="mt-1 font-mono text-xs text-muted-foreground">@{member.raUsername}</p>
              </div>
              <div className="font-mono text-xs text-muted-foreground sm:text-right">
                <span className="text-foreground">{member.friendRankOnes}</span> lead
                {member.friendRankOnes === 1 ? "" : "s"}
                <span className="mx-2 text-border">·</span>
                {member.boardsWithScore} scored
              </div>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
