import { FormattedSyncTime } from "@/components/formatted-sync-time";

type Props = {
  at?: string | null;
  prefix?: string;
};

export function LastSyncedLabel({ at, prefix = "Last synced" }: Props) {
  return (
    <p className="text-xs text-muted-foreground">
      {prefix}:{" "}
      {at ? <FormattedSyncTime value={at} /> : <span>Never synced</span>}
    </p>
  );
}
