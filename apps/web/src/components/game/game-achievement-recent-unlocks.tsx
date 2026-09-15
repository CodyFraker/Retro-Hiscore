import { ImageOff } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Badge } from "@/components/ui/badge";
import type { GameAchievementRecentUnlockDto } from "@/generated/api-client";

type Props = {
  unlocks: GameAchievementRecentUnlockDto[];
};

export function GameAchievementRecentUnlocks({ unlocks }: Props) {
  if (unlocks.length === 0) {
    return null;
  }

  return (
    <ul className="divide-y divide-border rounded-md border border-border">
      {unlocks.map((u) => (
        <li key={`${u.memberId}-${u.raAchievementId}-${u.dateEarned}`} className="flex gap-3 p-3">
          <div className="relative h-10 w-10 shrink-0 overflow-hidden rounded bg-secondary/40">
            {u.badgeUrl ? (
              <Image src={u.badgeUrl} alt="" fill className="object-contain p-0.5" sizes="40px" />
            ) : (
              <div className="flex h-full items-center justify-center text-muted-foreground">
                <ImageOff className="size-4" />
              </div>
            )}
          </div>
          <div className="min-w-0 flex-1 space-y-0.5">
            <p className="text-sm">
              <Link href={`/members/${encodeURIComponent(u.raUsername)}`} className="font-medium hover:text-[var(--accent-retro)]">
                {u.displayName}
              </Link>
              <span className="text-muted-foreground"> unlocked </span>
              <span className="font-medium">{u.title}</span>
              <span className="font-mono text-xs text-muted-foreground"> +{u.points}</span>
              {u.hardcoreEarned ? (
                <Badge variant="outline" className="ml-2 text-[10px] uppercase">
                  HC
                </Badge>
              ) : null}
            </p>
            {u.dateEarned ? (
              <p className="text-xs text-muted-foreground">
                <FormattedSyncTime value={u.dateEarned} />
              </p>
            ) : null}
          </div>
        </li>
      ))}
    </ul>
  );
}
