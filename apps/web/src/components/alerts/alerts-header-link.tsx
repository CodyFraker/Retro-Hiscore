import Link from "next/link";
import { Bell } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type Props = {
  unreadCount: number;
  className?: string;
};

export function AlertsHeaderLink({ unreadCount, className }: Props) {
  return (
    <Button
      variant="ghost"
      size="icon"
      className={cn("relative", className)}
      render={<Link href="/alerts" aria-label={`Alerts${unreadCount > 0 ? `, ${unreadCount} unread` : ""}`} />}
    >
      <Bell className="size-5" />
      {unreadCount > 0 ? (
        <span className="absolute -right-0.5 -top-0.5 flex min-h-4 min-w-4 items-center justify-center rounded-full bg-[var(--accent-retro)] px-1 text-[10px] font-medium text-primary-foreground">
          {unreadCount > 99 ? "99+" : unreadCount}
        </span>
      ) : null}
    </Button>
  );
}
