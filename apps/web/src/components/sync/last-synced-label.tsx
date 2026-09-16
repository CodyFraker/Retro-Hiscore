import { FormattedSyncTime } from "@/components/formatted-sync-time";

type Props = {
  at?: string | null;
  prefix?: string;
  emptyLabel?: string;
};

export function LastSyncedLabel({
  at,
  prefix = "Last synced",
  emptyLabel = "Never synced",
}: Props) {
  return (
    <p className="text-xs text-muted-foreground">
      {prefix}:{" "}
      {at ? <FormattedSyncTime value={at} /> : <span>{emptyLabel}</span>}
    </p>
  );
}
