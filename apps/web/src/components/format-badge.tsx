import { Badge } from "@/components/ui/badge";

type Props = {
  format?: string | null;
  rankAsc?: boolean;
};

export function FormatBadge({ format, rankAsc }: Props) {
  if (!format && rankAsc == null) {
    return null;
  }

  const label = [format, rankAsc ? "lower wins" : rankAsc === false ? "higher wins" : null]
    .filter(Boolean)
    .join(" · ");

  return (
    <Badge variant="outline" className="font-mono text-[10px] uppercase tracking-wide">
      {label}
    </Badge>
  );
}
