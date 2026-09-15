import Link from "next/link";
import type { AdminOpsDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { formatSyncTime } from "@/lib/format";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

type Props = {
  ops: AdminOpsDto;
  hangfireDashboardHref: string;
  showScheduler?: boolean;
};

export function AdminOpsView({ ops, hangfireDashboardHref, showScheduler = true }: Props) {
  const { memberCoverageSummary, members, scheduler, config } = ops;

  return (
    <div className="space-y-8">
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

      {showScheduler ? (
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
      ) : null}
    </div>
  );
}
