import Image from "next/image";
import Link from "next/link";
import { Pin } from "lucide-react";
import type { MemberPinDto } from "@/generated/api-client";

type Props = {
  pins: MemberPinDto[];
};

export function PinnedGamesStrip({ pins }: Props) {
  if (pins.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <div className="flex items-center gap-2 text-sm font-medium">
        <Pin className="size-4 text-[var(--accent-retro)]" />
        Pinned games
      </div>
      <div className="flex gap-3 overflow-x-auto pb-1">
        {pins.map((pin) => {
          const art = pin.imageBoxArtUrl ?? pin.imageIconUrl;
          return (
            <Link
              key={pin.raGameId}
              href={`/games/${pin.raGameId}`}
              className="flex w-36 shrink-0 flex-col gap-2 rounded border border-border bg-card p-2 transition-colors hover:border-[var(--accent-retro)]/50"
            >
              <div className="relative aspect-square overflow-hidden rounded bg-secondary/40">
                {art ? (
                  <Image src={art} alt="" fill className="object-cover" sizes="144px" />
                ) : (
                  <div className="flex h-full items-center justify-center text-xs text-muted-foreground">No art</div>
                )}
              </div>
              <span className="line-clamp-2 text-xs font-medium leading-snug">{pin.title}</span>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
