import { Badge } from "@/components/ui/badge";
import type { RaGameTitleModTag } from "@/lib/ra-game-title";

type Props = {
  modTags: RaGameTitleModTag[];
};

const badgeClassName =
  "h-auto rounded-md px-2 py-0.5 text-xs font-normal lowercase text-secondary-foreground";

export function RaGameModBadges({ modTags }: Props) {
  if (modTags.length === 0) {
    return null;
  }

  return (
    <span className="flex shrink-0 flex-wrap items-center gap-1.5">
      {modTags.map((tag) => (
        <Badge key={tag} variant="secondary" className={badgeClassName}>
          {tag}
        </Badge>
      ))}
    </span>
  );
}
