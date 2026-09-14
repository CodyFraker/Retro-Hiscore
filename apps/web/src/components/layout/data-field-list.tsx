import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

export type DataField = {
  label: ReactNode;
  value: ReactNode;
};

type Props = {
  fields: DataField[];
  className?: string;
};

export function DataFieldList({ fields, className }: Props) {
  if (fields.length === 0) {
    return null;
  }

  return (
    <div className={cn("flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground", className)}>
      {fields.map((field, index) => (
        <span key={index}>
          {field.label}{" "}
          <span className="text-foreground">{field.value}</span>
        </span>
      ))}
    </div>
  );
}
