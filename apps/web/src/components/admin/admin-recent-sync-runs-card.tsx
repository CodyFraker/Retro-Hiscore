import Link from "next/link";
import type { AdminOpsDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { formatSyncTime } from "@/lib/format";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

type Props = {
  recentRuns: AdminOpsDto["recentRuns"];
  hangfireDashboardHref: string;
};

function formatKind(kind: string) {
  switch (kind) {
    case "LeaderboardScores":
      return "Leaderboard scores";
    case "MemberRank":
      return "Member RA rank";
    case "MemberActivity":
      return "Member activity";
    case "MemberAchievements":
      return "Member achievements";
    case "GameMetadata":
      return "Game metadata";
    case "ConsoleIcons":
      return "Console icons";
    default:
      return kind;
  }
}

function statusBadgeClass(status: string) {
  switch (status) {
    case "Succeeded":
      return "text-emerald-600 dark:text-emerald-400";
    case "PartialSuccess":
      return "text-amber-600 dark:text-amber-400";
    case "Failed":
      return "text-destructive";
    case "Running":
      return "text-sky-600 dark:text-sky-400";
    default:
      return "text-muted-foreground";
  }
}

function formatDuration(startedAt: string, finishedAt: string | null | undefined) {
  if (!finishedAt) {
    return "—";
  }
  const ms = new Date(finishedAt).getTime() - new Date(startedAt).getTime();
  if (ms < 1000) {
    return `${ms}ms`;
  }
  const seconds = Math.round(ms / 1000);
  if (seconds < 60) {
    return `${seconds}s`;
  }
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`;
}

function truncateError(error: string | null | undefined, max = 80) {
  if (!error) {
    return null;
  }
  if (error.length <= max) {
    return error;
  }
  return `${error.slice(0, max)}…`;
}

type SyncRunRow = AdminOpsDto["recentRuns"][number];

function formatRunTarget(run: SyncRunRow) {
  if (run.gameTitle) {
    return run.raGameId != null ? `${run.gameTitle} (#${run.raGameId})` : run.gameTitle;
  }

  if (run.memberDisplayName) {
    return `Member: ${run.memberDisplayName}`;
  }

  return "—";
}

function RunErrorCell({ error }: { error: string | null | undefined }) {
  if (!error) {
    return <span className="text-muted-foreground">—</span>;
  }

  return (
    <details>
      <summary className="cursor-pointer text-xs">{truncateError(error)}</summary>
      <p className="mt-1 whitespace-pre-wrap break-words text-xs text-muted-foreground">{error}</p>
    </details>
  );
}

export function AdminRecentSyncRunsCard({ recentRuns, hangfireDashboardHref }: Props) {
  const rows = recentRuns.slice(0, 8);

  return (
    <Card className="flex h-full flex-col">
      <CardHeader className="pb-2">
        <CardTitle>Recent sync runs</CardTitle>
        <CardDescription>Latest jobs · member refreshes run via Hangfire</CardDescription>
      </CardHeader>
      <CardContent className="min-h-0 flex-1 overflow-y-auto">
        <ResponsiveTable
          rows={rows}
          rowKey={(run) => run.id}
          emptyMessage={<p className="text-sm text-muted-foreground">No sync runs yet.</p>}
          columns={[
            { header: "Kind", render: (run) => formatKind(run.kind) },
            { header: "Target", render: (run) => formatRunTarget(run) },
            {
              header: "Status",
              render: (run) => (
                <span className={statusBadgeClass(run.status)}>{run.status}</span>
              ),
            },
            { header: "Started", render: (run) => formatSyncTime(run.startedAt) },
          ]}
          renderMobileCard={(run) => (
            <li
              key={run.id}
              className={cn(
                "rounded border border-border bg-card p-3 text-sm",
                (run.status === "Failed" || run.status === "PartialSuccess") && "bg-destructive/5",
              )}
            >
              <p className="font-medium">{formatKind(run.kind)}</p>
              <DataFieldList
                className="mt-2"
                fields={[
                  { label: "Target", value: formatRunTarget(run) },
                  { label: "Status", value: run.status },
                  { label: "Started", value: formatSyncTime(run.startedAt) },
                  {
                    label: "Duration",
                    value: formatDuration(run.startedAt, run.finishedAt),
                  },
                ]}
              />
              <div className="mt-2">
                <RunErrorCell error={run.error} />
              </div>
            </li>
          )}
        />
      </CardContent>
      <div className="border-t border-border px-6 py-3">
        <Link
          href={hangfireDashboardHref}
          className="text-sm text-[var(--accent-retro)] hover:underline"
          target="_blank"
          rel="noopener noreferrer"
        >
          Open Hangfire dashboard
        </Link>
      </div>
    </Card>
  );
}
