"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useTransition } from "react";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { MemberAvatar } from "@/components/members/member-avatar";
import { Button } from "@/components/ui/button";
import type { MemberAlertDto } from "@/generated/api-client";
import { markAlertReadAction, markAllAlertsReadAction } from "@/lib/actions/alerts";
import { cn } from "@/lib/utils";

type Props = {
  items: MemberAlertDto[];
  unreadCount: number;
};

function alertBody(alert: MemberAlertDto): string {
  const actor = alert.actorDisplayName ?? "Someone";
  switch (alert.kind) {
    case "YouTookBoard":
      return `You took friend rank 1 on ${alert.leaderboardTitle ?? "a board"} in ${alert.gameTitle ?? "a game"}.`;
    case "YouWerePassed":
      return `${actor} passed you on ${alert.leaderboardTitle ?? "a board"} in ${alert.gameTitle ?? "a game"}.`;
    case "FriendAchievement":
      return `${actor} unlocked ${alert.achievementTitle ?? "an achievement"} in ${alert.gameTitle ?? "a game"}.`;
    default:
      return "Activity update";
  }
}

function alertHref(alert: MemberAlertDto): string | null {
  if (alert.raLeaderboardId != null) {
    return `/leaderboards/${alert.raLeaderboardId}`;
  }
  if (alert.raGameId != null) {
    return `/games/${alert.raGameId}`;
  }
  if (alert.actorRaUsername) {
    return `/members/${encodeURIComponent(alert.actorRaUsername)}`;
  }
  return null;
}

export function AlertsList({ items, unreadCount }: Props) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();

  return (
    <div className="space-y-4">
      {unreadCount > 0 ? (
        <div className="flex justify-end">
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={pending}
            onClick={() => {
              startTransition(async () => {
                await markAllAlertsReadAction();
                router.refresh();
              });
            }}
          >
            Mark all read
          </Button>
        </div>
      ) : null}
      {items.length === 0 ? (
        <p className="text-sm text-muted-foreground">No alerts yet.</p>
      ) : (
        <ul className="divide-y divide-border rounded border border-border">
          {items.map((alert) => {
            const href = alertHref(alert);
            const unread = alert.readAt == null;
            const content = (
              <div className="flex gap-3 px-4 py-3">
                {alert.actorAvatarUrl || alert.actorDisplayName ? (
                  <MemberAvatar
                    avatarUrl={alert.actorAvatarUrl}
                    displayName={alert.actorDisplayName ?? "?"}
                    size={32}
                  />
                ) : null}
                <div className="min-w-0 flex-1 space-y-1">
                  <p className={cn("text-sm", unread && "font-medium")}>{alertBody(alert)}</p>
                  <p className="text-xs text-muted-foreground">
                    <FormattedSyncTime value={alert.occurredAt} />
                  </p>
                </div>
              </div>
            );

            return (
              <li key={alert.id}>
                {href ? (
                  <Link
                    href={href}
                    className="block hover:bg-secondary/40"
                    onClick={() => {
                      if (unread) {
                        void markAlertReadAction(alert.id);
                      }
                    }}
                  >
                    {content}
                  </Link>
                ) : (
                  <div className={cn(unread && "bg-secondary/20")}>{content}</div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
