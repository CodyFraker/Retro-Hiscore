import { ExternalLink } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type Props = {
  raGameId: number;
};

export function RetroachievementsLink({ raGameId }: Props) {
  return (
    <a
      href={`https://retroachievements.org/game/${raGameId}`}
      target="_blank"
      rel="noopener noreferrer"
      className={cn(buttonVariants({ variant: "outline" }), "w-full sm:w-auto")}
    >
      View on RetroAchievements
      <ExternalLink className="size-4" />
    </a>
  );
}
