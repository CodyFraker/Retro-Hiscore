import Link from "next/link";
import type { AdminOpsDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { PageHero } from "@/components/layout/page-hero";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { formatSyncTime } from "@/lib/format";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

type Props = {
  ops: AdminOpsDto;
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

function overallStatusClass(status: string) {
  switch (status) {
    case "OK":
      return "text-emerald-600 dark:text-emerald-400";
    case "Degraded":
      return "text-amber-600 dark:text-amber-400";
    default:
      return "text-destructive";
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

export function AdminOpsView({ ops, hangfireDashboardHref }: Props) {
  const { health, recentRuns, memberCoverageSummary, members, scheduler, config } = ops;

  return (
    <div className="mx-auto max-w-6xl space-y-8">
      <PageHero
        title="Admin · Sync & jobs"
        description="Health, run history, member API key coverage, and scheduler context."
      />

      <Card>
        <CardHeader>
          <CardTitle>Health</CardTitle>
          <CardDescription>Leaderboard sync at a glance</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <div>
            <p className="text-xs text-muted-foreground">Overall</p>
            <p className={`font-medium ${overallStatusClass(health.overallStatus)}`}>
              {health.overallStatus}
            </p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Sync in progress</p>
            <p className="font-medium">{health.leaderboardSyncInProgress ? "Yes" : "No"}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Last full success</p>
            <p className="font-medium">{formatSyncTime(health.lastSuccessfulLeaderboardSyncAt)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Interval</p>
            <p className="font-medium">{health.intervalMinutes} min</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Next scheduled (est.)</p>
            <p className="font-medium">{formatSyncTime(health.estimatedNextScheduledSyncAt)}</p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Manual refresh cooldown</p>
            <p className="font-medium">
              {health.manualCooldownUntil
                ? `Until ${formatSyncTime(health.manualCooldownUntil)}`
                : "Ready"}
            </p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Recent sync runs</CardTitle>
          <CardDescription>Last 15 runs · per-game and member refreshes run via Hangfire jobs</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveTable
            rows={recentRuns}
            rowKey={(run) => run.id}
            emptyMessage={<p className="text-sm text-muted-foreground">No sync runs yet.</p>}
            columns={[
              { header: "Kind", render: (run) => formatKind(run.kind) },
              {
                header: "Game",
                render: (run) =>
                  run.raGameId != null
                    ? `${run.gameTitle ?? "Game"} (#${run.raGameId})`
                    : "—",
              },
              { header: "Trigger", render: (run) => run.trigger },
              {
                header: "Status",
                render: (run) => (
                  <span className={statusBadgeClass(run.status)}>{run.status}</span>
                ),
              },
              { header: "Started", render: (run) => formatSyncTime(run.startedAt) },
              {
                header: "Duration",
                render: (run) => formatDuration(run.startedAt, run.finishedAt),
              },
              {
                header: "Error",
                cellClassName: "max-w-xs",
                render: (run) => <RunErrorCell error={run.error} />,
              },
            ]}
            renderMobileCard={(run) => (
              <li
                key={run.id}
                className={cn(
                  "rounded border border-border bg-card p-4 text-sm",
                  (run.status === "Failed" || run.status === "PartialSuccess") && "bg-destructive/5",
                )}
              >
                <p className="font-medium">{formatKind(run.kind)}</p>
                <DataFieldList
                  className="mt-2"
                  fields={[
                    { label: "Trigger", value: run.trigger },
                    { label: "Status", value: run.status },
                    { label: "Started", value: formatSyncTime(run.startedAt) },
                    {
                      label: "Duration",
                      value: formatDuration(run.startedAt, run.finishedAt),
                    },
                  ]}
                />
                <div className="mt-2">
                  <p className="text-xs text-muted-foreground">Error</p>
                  <RunErrorCell error={run.error} />
                </div>
              </li>
            )}
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Member sync coverage</CardTitle>
          <CardDescription>
            {memberCoverageSummary.membersWithApiKey} of {memberCoverageSummary.totalMembers} members
            have an API key configured
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveTable
            rows={members}
            rowKey={(member) => member.raUsername}
            columns={[
              {
                header: "Member",
                render: (member) => (
                  <>
                    <span className="font-medium">{member.displayName}</span>
                    <span className="block font-mono text-xs text-muted-foreground">
                      @{member.raUsername}
                    </span>
                  </>
                ),
              },
              { header: "API key", render: (member) => (member.hasApiKey ? "Yes" : "No") },
              { header: "Boards", render: (member) => member.boardsWithScore },
              {
                header: "Last entry sync",
                render: (member) => formatSyncTime(member.lastEntrySyncedAt),
              },
            ]}
            renderMobileCard={(member) => (
              <li
                key={member.raUsername}
                className={cn(
                  "rounded border border-border bg-card p-4 text-sm",
                  !member.hasApiKey && "bg-amber-500/5",
                )}
              >
                <p className="font-medium">{member.displayName}</p>
                <p className="font-mono text-xs text-muted-foreground">@{member.raUsername}</p>
                <DataFieldList
                  className="mt-2"
                  fields={[
                    { label: "API key", value: member.hasApiKey ? "Yes" : "No" },
                    { label: "Boards", value: member.boardsWithScore },
                    {
                      label: "Last entry sync",
                      value: formatSyncTime(member.lastEntrySyncedAt),
                    },
                  ]}
                />
              </li>
            )}
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Scheduler & RA pool</CardTitle>
          <CardDescription>Hangfire recurring jobs and API key pool size</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-3">
            <div>
              <p className="text-xs text-muted-foreground">Shared catalog key</p>
              <p className="font-medium">{config.sharedCatalogKeyConfigured ? "Configured" : "Missing"}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Distinct member keys</p>
              <p className="font-medium">{config.distinctMemberApiKeys}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Keys in failover pool</p>
              <p className="font-medium">{config.keysInPool}</p>
            </div>
          </div>

          {scheduler.recurringJobs.length > 0 && (
            <ResponsiveTable
              rows={scheduler.recurringJobs}
              rowKey={(job) => job.jobId}
              columns={[
                {
                  header: "Job",
                  cellClassName: "font-mono text-xs",
                  render: (job) => job.jobId,
                },
                {
                  header: "Cron",
                  cellClassName: "text-xs",
                  render: (job) => job.cron ?? "—",
                },
                { header: "Last run", render: (job) => formatSyncTime(job.lastExecution) },
                { header: "Next run", render: (job) => formatSyncTime(job.nextExecution) },
                { header: "Last state", render: (job) => job.lastJobState ?? "—" },
              ]}
              renderMobileCard={(job) => (
                <li key={job.jobId} className="rounded border border-border bg-card p-4 text-sm">
                  <p className="break-all font-mono text-xs">{job.jobId}</p>
                  <DataFieldList
                    className="mt-2"
                    fields={[
                      { label: "Cron", value: job.cron ?? "—" },
                      { label: "Last run", value: formatSyncTime(job.lastExecution) },
                      { label: "Next run", value: formatSyncTime(job.nextExecution) },
                      { label: "Last state", value: job.lastJobState ?? "—" },
                    ]}
                  />
                </li>
              )}
            />
          )}

          <p>
            <Link
              href={hangfireDashboardHref}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-[var(--accent-retro)] underline-offset-4 hover:underline"
            >
              Open Hangfire dashboard
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
