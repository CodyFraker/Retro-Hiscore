import type { AdminOpsDto } from "@/generated/api-client";
import { formatSyncTime } from "@/lib/format";
import { cn } from "@/lib/utils";

type Props = {
  ops: AdminOpsDto;
};

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

export function AdminSyncHealthStrip({ ops }: Props) {
  const { health } = ops;

  return (
    <div className="flex flex-wrap items-center gap-x-6 gap-y-2 rounded-md border border-border bg-card px-4 py-3 text-sm">
      <div>
        <span className="text-muted-foreground">Overall </span>
        <span className={cn("font-medium", overallStatusClass(health.overallStatus))}>
          {health.overallStatus}
        </span>
      </div>
      <div>
        <span className="text-muted-foreground">Sync in progress </span>
        <span className="font-medium">{health.leaderboardSyncInProgress ? "Yes" : "No"}</span>
      </div>
      <div>
        <span className="text-muted-foreground">Manual cooldown </span>
        <span className="font-medium">
          {health.manualCooldownUntil
            ? `Until ${formatSyncTime(health.manualCooldownUntil)}`
            : "Ready"}
        </span>
      </div>
      <div>
        <span className="text-muted-foreground">Last full success </span>
        <span className="font-medium">{formatSyncTime(health.lastSuccessfulLeaderboardSyncAt)}</span>
      </div>
    </div>
  );
}
