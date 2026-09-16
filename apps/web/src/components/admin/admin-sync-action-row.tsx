import type { ReactNode } from "react";

type Props = {
  title: string;
  description: string;
  lastRun: ReactNode;
  actions: ReactNode;
  footerMessage?: string | null;
};

export function AdminSyncActionRow({
  title,
  description,
  lastRun,
  actions,
  footerMessage,
}: Props) {
  return (
    <div className="flex flex-col gap-3 border-b border-border py-4 last:border-b-0 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0 flex-1 space-y-1">
        <p className="font-medium">{title}</p>
        <p className="text-sm text-muted-foreground">{description}</p>
        <p className="text-xs text-muted-foreground">Last run: {lastRun}</p>
        {footerMessage ? (
          <p className="text-xs text-muted-foreground">{footerMessage}</p>
        ) : null}
      </div>
      <div className="flex shrink-0 flex-col items-stretch gap-2 sm:items-end">{actions}</div>
    </div>
  );
}
