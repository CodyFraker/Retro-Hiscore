import Link from "next/link";
import { Button } from "@/components/ui/button";

type Props = {
  raLeaderboardId: number;
  total: number;
  offset: number;
  limit: number;
  historyPage: number;
};

function historyPageHref(raLeaderboardId: number, page: number) {
  if (page <= 1) {
    return `/leaderboards/${raLeaderboardId}`;
  }
  return `/leaderboards/${raLeaderboardId}?historyPage=${page}`;
}

export function LeaderboardHistoryPagination({
  raLeaderboardId,
  total,
  offset,
  limit,
  historyPage,
}: Props) {
  if (total <= limit) {
    return null;
  }

  const pageCount = Math.max(1, Math.ceil(total / limit));
  const prevPage = historyPage > 1 ? historyPage - 1 : null;
  const nextPage = historyPage < pageCount ? historyPage + 1 : null;
  const rangeStart = total === 0 ? 0 : offset + 1;
  const rangeEnd = Math.min(offset + limit, total);

  return (
    <div className="flex flex-col gap-3 border-t border-border pt-4 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm text-muted-foreground">
        Showing {rangeStart}–{rangeEnd} of {total}
      </p>
      <div className="flex items-center gap-2">
        {prevPage ? (
          <Button
            variant="outline"
            size="sm"
            render={<Link href={historyPageHref(raLeaderboardId, prevPage)} />}
          >
            Previous
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>Previous</Button>
        )}
        <span className="text-sm text-muted-foreground tabular-nums">
          Page {historyPage} of {pageCount}
        </span>
        {nextPage ? (
          <Button
            variant="outline"
            size="sm"
            render={<Link href={historyPageHref(raLeaderboardId, nextPage)} />}
          >
            Next
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>Next</Button>
        )}
      </div>
    </div>
  );
}
