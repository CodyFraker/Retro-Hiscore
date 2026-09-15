import Link from "next/link";
import { Button } from "@/components/ui/button";
import { buildTrackedGamesQueryString } from "@/lib/tracked-games-params";
import type { DashboardGameSortKey } from "@/lib/dashboard-games";

type Props = {
  basePath: string;
  total: number;
  offset: number;
  limit: number;
  query: string;
  sort: DashboardGameSortKey;
};

export function TrackedGamesPagination({
  basePath,
  total,
  offset,
  limit,
  query,
  sort,
}: Props) {
  if (total <= limit) {
    return null;
  }

  const page = Math.floor(offset / limit) + 1;
  const pageCount = Math.max(1, Math.ceil(total / limit));
  const prevPage = page > 1 ? page - 1 : null;
  const nextPage = page < pageCount ? page + 1 : null;

  const linkForPage = (targetPage: number) =>
    `${basePath}${buildTrackedGamesQueryString({ page: targetPage, q: query, sort })}`;

  const rangeStart = total === 0 ? 0 : offset + 1;
  const rangeEnd = Math.min(offset + limit, total);

  return (
    <div className="flex flex-col gap-3 border-t border-border px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
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
