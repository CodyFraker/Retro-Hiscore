import Link from "next/link";
import type { AdminOpsDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

type Props = {
  ops: AdminOpsDto;
  hangfireDashboardHref: string;
};

function formatKind(kind: string) {
  switch (kind) {
    case "LeaderboardScores":
      return "Leaderboard scores";
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

export function AdminOpsView({ ops, hangfireDashboardHref }: Props) {
  const { health, recentRuns, memberCoverageSummary, members, scheduler, config } = ops;

  return (
    <div className="mx-auto max-w-6xl space-y-8">
      <section className="space-y-2">
        <h1 className="font-[family-name:var(--font-display)] text-3xl tracking-tight text-[var(--accent-retro)]">
          Admin · Sync & jobs
        </h1>
        <p className="text-muted-foreground">
          Health, run history, member API key coverage, and scheduler context.
        </p>
      </section>

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
            <p className="font-medium">
              {formatSyncTime(health.lastSuccessfulLeaderboardSyncAt)}
            </p>
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
          <CardDescription>Last 15 runs · manual refreshes run inline, not via Hangfire</CardDescription>
        </CardHeader>
        <CardContent>
          {recentRuns.length === 0 ? (
            <p className="text-sm text-muted-foreground">No sync runs yet.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Kind</TableHead>
                  <TableHead>Trigger</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Started</TableHead>
                  <TableHead>Duration</TableHead>
                  <TableHead>Error</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recentRuns.map((run) => (
                  <TableRow
                    key={run.id}
                    className={
                      run.status === "Failed" || run.status === "PartialSuccess"
                        ? "bg-destructive/5"
                        : undefined
                    }
                  >
                    <TableCell>{formatKind(run.kind)}</TableCell>
                    <TableCell>{run.trigger}</TableCell>
                    <TableCell className={statusBadgeClass(run.status)}>{run.status}</TableCell>
                    <TableCell>{formatSyncTime(run.startedAt)}</TableCell>
                    <TableCell>{formatDuration(run.startedAt, run.finishedAt)}</TableCell>
                    <TableCell className="max-w-xs">
                      {run.error ? (
                        <details>
                          <summary className="cursor-pointer text-xs">
                            {truncateError(run.error)}
                          </summary>
                          <p className="mt-1 whitespace-pre-wrap break-words text-xs text-muted-foreground">
                            {run.error}
                          </p>
                        </details>
                      ) : (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Member sync coverage</CardTitle>
          <CardDescription>
            {memberCoverageSummary.membersWithApiKey} of {memberCoverageSummary.totalMembers}{" "}
            members have an API key configured
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Member</TableHead>
                <TableHead>API key</TableHead>
                <TableHead>Boards</TableHead>
                <TableHead>Last entry sync</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {members.map((member) => (
                <TableRow key={member.raUsername} className={!member.hasApiKey ? "bg-amber-500/5" : undefined}>
                  <TableCell>
                    <span className="font-medium">{member.displayName}</span>
                    <span className="block font-mono text-xs text-muted-foreground">
                      @{member.raUsername}
                    </span>
                  </TableCell>
                  <TableCell>{member.hasApiKey ? "Yes" : "No"}</TableCell>
                  <TableCell>{member.boardsWithScore}</TableCell>
                  <TableCell>{formatSyncTime(member.lastEntrySyncedAt)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
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
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Job</TableHead>
                  <TableHead>Cron</TableHead>
                  <TableHead>Last run</TableHead>
                  <TableHead>Next run</TableHead>
                  <TableHead>Last state</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {scheduler.recurringJobs.map((job) => (
                  <TableRow key={job.jobId}>
                    <TableCell className="font-mono text-xs">{job.jobId}</TableCell>
                    <TableCell className="text-xs">{job.cron ?? "—"}</TableCell>
                    <TableCell>{formatSyncTime(job.lastExecution)}</TableCell>
                    <TableCell>{formatSyncTime(job.nextExecution)}</TableCell>
                    <TableCell>{job.lastJobState ?? "—"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
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
