import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

export type StatGridItem = {
  label: string;
  value: ReactNode;
  compact?: boolean;
};

type Props = {
  items: StatGridItem[];
  className?: string;
  columnsClassName?: string;
};

export function StatGrid({
  items,
  className,
  columnsClassName = "grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6",
}: Props) {
  return (
    <section className={cn(columnsClassName, className)}>
      {items.map((item) => (
        <div key={item.label} className="min-w-0 rounded border border-border bg-secondary/20 px-3 py-3">
          <p className="text-xs text-muted-foreground">{item.label}</p>
          <p
            className={cn(
              "mt-1 font-mono text-foreground",
              item.compact ? "truncate text-xs" : "line-clamp-2 text-base sm:text-lg",
            )}
          >
            {item.value}
          </p>
        </div>
      ))}
    </section>
  );
}
