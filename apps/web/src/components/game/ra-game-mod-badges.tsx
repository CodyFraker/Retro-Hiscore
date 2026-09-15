import { Badge } from "@/components/ui/badge";
import type { RaGameTitleModTag } from "@/lib/ra-game-title";

type Props = {
  modTags: RaGameTitleModTag[];
};

export function RaGameModBadges({ modTags }: Props) {
  if (modTags.length === 0) {
    return null;
  }

  return (
    <span className="flex shrink-0 flex-wrap items-center gap-1">
      {modTags.map((tag) => (
        <Badge key={tag} variant="outline" className="text-[10px] lowercase">
          {tag}
        </Badge>
      ))}
    </span>
  );
}
