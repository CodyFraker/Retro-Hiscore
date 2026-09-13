import { Medal } from "lucide-react";
import { formatFriendRank } from "@/lib/format";
import { cn } from "@/lib/utils";

type Props = {
  rank: number | null | undefined;
  className?: string;
  iconClassName?: string;
};

const podiumClass: Record<1 | 2 | 3, string> = {
  1: "text-amber-400",
  2: "text-slate-300",
  3: "text-amber-700",
};

export function FriendRank({ rank, className, iconClassName }: Props) {
  const label = formatFriendRank(rank);

  if (rank !== 1 && rank !== 2 && rank !== 3) {
    return <span className={className}>{label}</span>;
  }

  return (
    <span className={cn("inline-flex items-center gap-1", podiumClass[rank], className)}>
      <Medal aria-hidden className={cn("size-3.5 shrink-0", iconClassName)} />
      {label}
    </span>
  );
}
