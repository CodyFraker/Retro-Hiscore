"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminSyncSettingsDto } from "@/generated/api-client";
import { SyncRecurringJobIds } from "@/lib/sync-recurring-job-ids";
import { patchAdminSyncSettingsAction } from "@/lib/actions/admin";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

type Props = {
  initialSettings: AdminSyncSettingsDto;
};

type LeaderboardForm = {
  hotIntervalMinutes: string;
  coldIntervalMinutes: string;
  hotActivityWindowHours: string;
};

type RecurringJobForm = {
  jobId: string;
  displayName: string;
  intervalMinutes: string;
  intervalDays: string;
  usesDays: boolean;
};

function toLeaderboardForm(settings: AdminSyncSettingsDto): LeaderboardForm {
  return {
    hotIntervalMinutes: String(settings.leaderboard.hotIntervalMinutes),
    coldIntervalMinutes: String(settings.leaderboard.coldIntervalMinutes),
    hotActivityWindowHours: String(settings.leaderboard.hotActivityWindowHours),
  };
}

function toRecurringForms(settings: AdminSyncSettingsDto): RecurringJobForm[] {
  return settings.recurringJobs.map((job) => ({
    jobId: job.jobId,
    displayName: job.displayName,
    intervalMinutes: job.intervalMinutes != null ? String(job.intervalMinutes) : "",
    intervalDays: job.intervalDays != null ? String(job.intervalDays) : "",
    usesDays: job.jobId === SyncRecurringJobIds.GameMetadata,
  }));
}

export function AdminSyncSettingsSection({ initialSettings }: Props) {
  const router = useRouter();
  const [leaderboard, setLeaderboard] = useState(() => toLeaderboardForm(initialSettings));
  const [recurringJobs, setRecurringJobs] = useState(() => toRecurringForms(initialSettings));
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Sync schedules</CardTitle>
        <CardDescription>
          Hangfire recurring jobs and per-game hot/cold leaderboard intervals. Changes apply immediately.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form
          className="space-y-6"
          onSubmit={(event) => {
            event.preventDefault();
            startTransition(async () => {
              setError(null);
              const hot = Number.parseInt(leaderboard.hotIntervalMinutes, 10);
              const cold = Number.parseInt(leaderboard.coldIntervalMinutes, 10);
              const windowHours = Number.parseInt(leaderboard.hotActivityWindowHours, 10);
              if ([hot, cold, windowHours].some((n) => Number.isNaN(n))) {
                setError("Leaderboard intervals must be valid numbers.");
                return;
              }

              const recurringPayload = recurringJobs.map((job) => {
                if (job.usesDays) {
                  const days = Number.parseInt(job.intervalDays, 10);
                  if (Number.isNaN(days)) {
                    return null;
                  }
                  return { jobId: job.jobId, intervalMinutes: null, intervalDays: days };
                }

                const minutes = Number.parseInt(job.intervalMinutes, 10);
                if (Number.isNaN(minutes)) {
                  return null;
                }
                return { jobId: job.jobId, intervalMinutes: minutes, intervalDays: null };
              });

              if (recurringPayload.some((item) => item === null)) {
                setError("Recurring job intervals must be valid numbers.");
                return;
              }

              const result = await patchAdminSyncSettingsAction({
                leaderboard: {
                  hotIntervalMinutes: hot,
                  coldIntervalMinutes: cold,
                  hotActivityWindowHours: windowHours,
                },
                recurringJobs: recurringPayload.filter((item) => item !== null),
              });

              if (!result.ok) {
                setError(result.error);
                return;
              }

              router.refresh();
            });
          }}
        >
          <div className="space-y-3">
            <h3 className="text-sm font-medium">Leaderboard policy</h3>
            <p className="text-xs text-muted-foreground">
              The dispatcher runs on its own schedule; these values control how often each tracked game is eligible
              for sync (hot vs cold based on recent play).
            </p>
            <div className="grid gap-3 sm:grid-cols-3">
              <label className="space-y-1 text-sm">
                <span className="text-muted-foreground">Hot interval (minutes)</span>
                <input
                  type="number"
                  min={1}
                  max={1440}
                  value={leaderboard.hotIntervalMinutes}
                  onChange={(e) =>
                    setLeaderboard((prev) => ({ ...prev, hotIntervalMinutes: e.target.value }))
                  }
                  className="w-full rounded-md bg-input px-3 py-2 font-mono text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
                />
              </label>
              <label className="space-y-1 text-sm">
                <span className="text-muted-foreground">Cold interval (minutes)</span>
                <input
                  type="number"
                  min={1}
                  max={10080}
                  value={leaderboard.coldIntervalMinutes}
                  onChange={(e) =>
                    setLeaderboard((prev) => ({ ...prev, coldIntervalMinutes: e.target.value }))
                  }
                  className="w-full rounded-md bg-input px-3 py-2 font-mono text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
                />
              </label>
              <label className="space-y-1 text-sm">
                <span className="text-muted-foreground">Hot window (hours)</span>
                <input
                  type="number"
                  min={1}
                  value={leaderboard.hotActivityWindowHours}
                  onChange={(e) =>
                    setLeaderboard((prev) => ({ ...prev, hotActivityWindowHours: e.target.value }))
                  }
                  className="w-full rounded-md bg-input px-3 py-2 font-mono text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
                />
              </label>
            </div>
          </div>

          <div className="space-y-3">
            <h3 className="text-sm font-medium">Recurring Hangfire jobs</h3>
            <ul className="space-y-3">
              {recurringJobs.map((job, index) => (
                <li
                  key={job.jobId}
                  className="flex flex-col gap-2 rounded border border-border bg-card p-3 sm:flex-row sm:items-end sm:justify-between"
                >
                  <div>
                    <p className="font-medium">{job.displayName}</p>
                    <p className="font-mono text-xs text-muted-foreground">{job.jobId}</p>
                  </div>
                  <label className="space-y-1 text-sm sm:min-w-[10rem]">
                    <span className="text-muted-foreground">
                      {job.usesDays ? "Every N days" : "Every N minutes"}
                    </span>
                    <input
                      type="number"
                      min={1}
                      max={job.usesDays ? 365 : job.jobId === SyncRecurringJobIds.MemberRank ? 1440 : 60}
                      value={job.usesDays ? job.intervalDays : job.intervalMinutes}
                      onChange={(e) => {
                        const value = e.target.value;
                        setRecurringJobs((prev) =>
                          prev.map((row, i) => {
                            if (i !== index) {
                              return row;
                            }
                            return job.usesDays
                              ? { ...row, intervalDays: value }
                              : { ...row, intervalMinutes: value };
                          }),
                        );
                      }}
                      className="w-full rounded-md bg-input px-3 py-2 font-mono text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
                    />
                  </label>
                </li>
              ))}
            </ul>
          </div>

          {error && (
            <p className="rounded border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm">{error}</p>
          )}

          <Button type="submit" disabled={pending}>
            {pending ? "Saving…" : "Save sync schedules"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
