import { Clock } from "lucide-react";

type Props = {
  message?: string;
};

export function ChartEmptyState({
  message = "Charts unlock after the next sync.",
}: Props) {
  return (
    <div className="flex min-h-48 items-center justify-center gap-2 rounded border border-dashed border-border px-4 py-8 text-sm text-muted-foreground">
      <Clock className="size-4 shrink-0" />
      <p>{message}</p>
    </div>
  );
}
