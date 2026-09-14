import { ExternalLink } from "lucide-react";
import type { GameSourceDto } from "@/generated/api-client";
import { gameSourceTypeLabel } from "@/lib/game-source-labels";

type Props = {
  sources: GameSourceDto[];
};

export function GameSourcesSection({ sources }: Props) {
  if (sources.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h2 className="steam-section-heading">Get the game</h2>
      <p className="text-sm text-muted-foreground">
        Download mirrors for this title. Links open external file hosts.
      </p>
      <ul className="divide-y divide-border rounded border border-border">
        {sources.map((source) => (
          <li key={source.id} className="flex flex-col gap-1 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="min-w-0">
              <p className="font-medium">{gameSourceTypeLabel(source.sourceType, source.label)}</p>
              {source.note && <p className="text-sm text-muted-foreground">{source.note}</p>}
            </div>
            <a
              href={source.url}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1 text-sm text-[var(--accent-retro)] hover:underline"
            >
              Open link
              <ExternalLink className="size-3.5 shrink-0" />
            </a>
          </li>
        ))}
      </ul>
    </section>
  );
}
