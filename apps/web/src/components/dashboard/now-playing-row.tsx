import Link from "next/link";
import { MemberAvatar } from "@/components/members/member-avatar";
import type { NowPlayingItemDto } from "@/generated/api-client";

type Props = {
  items: NowPlayingItemDto[];
};

export function NowPlayingRow({ items }: Props) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section
      className="rounded border border-border bg-card/50 px-4 py-3"
      aria-label="Now playing"
    >
      <p className="mb-3 text-xs font-medium uppercase tracking-wide text-muted-foreground">Now playing</p>
      <ul className="flex flex-wrap gap-4">
        {items.map((item) => {
          const gameHref = item.isTracked
            ? `/games/${item.raGameId}`
            : `/members/${encodeURIComponent(item.raUsername)}`;
          return (
            <li key={item.memberId} className="flex min-w-0 max-w-xs items-center gap-2 text-sm">
              <span
                className="size-2 shrink-0 rounded-full bg-[var(--accent-retro)]"
                aria-hidden
              />
              <Link
                href={`/members/${encodeURIComponent(item.raUsername)}`}
                className="flex shrink-0 items-center gap-1.5 hover:text-[var(--accent-retro)]"
              >
                <MemberAvatar avatarUrl={item.avatarUrl} displayName={item.displayName} size={24} />
                <span className="truncate font-medium">{item.displayName}</span>
              </Link>
              <span className="text-muted-foreground">·</span>
              <Link href={gameHref} className="truncate text-muted-foreground hover:text-[var(--accent-retro)]">
                {item.gameTitle}
              </Link>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
