import Link from "next/link";
import { Button } from "@/components/ui/button";
import { buildAchievementsQueryString } from "@/lib/achievements-params";

type Props = {
  total: number;
  offset: number;
  limit: number;
  gameId?: number;
  member?: string;
};

export function AchievementsHubPagination({ total, offset, limit, gameId, member }: Props) {
  if (total <= limit) {
    return null;
  }

  const page = Math.floor(offset / limit) + 1;
  const pageCount = Math.max(1, Math.ceil(total / limit));
  const prevPage = page > 1 ? page - 1 : null;
  const nextPage = page < pageCount ? page + 1 : null;

  const linkForPage = (targetPage: number) =>
    `/achievements${buildAchievementsQueryString({
      page: targetPage,
      game: gameId,
      member,
    })}`;

  const rangeStart = total === 0 ? 0 : offset + 1;
  const rangeEnd = Math.min(offset + limit, total);

  return (
    <div className="flex flex-col gap-3 border-t border-border pt-4 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm text-muted-foreground">
        Showing {rangeStart}–{rangeEnd} of {total}
      </p>
      <div className="flex items-center gap-2">
        {prevPage ? (
          <Button variant="outline" size="sm" render={<Link href={linkForPage(prevPage)} />}>
            Previous
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>Previous</Button>
        )}
        <span className="text-sm text-muted-foreground tabular-nums">
          Page {page} of {pageCount}
        </span>
        {nextPage ? (
          <Button variant="outline" size="sm" render={<Link href={linkForPage(nextPage)} />}>
            Next
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>Next</Button>
        )}
      </div>
    </div>
  );
}
