import Link from "next/link";
import { Button } from "@/components/ui/button";

type Props = {
  total: number;
  offset: number;
  limit: number;
  standingsPage: number;
  standingsPageHref: (page: number) => string;
};

export function MemberStandingsPagination({
  total,
  offset,
  limit,
  standingsPage,
  standingsPageHref,
}: Props) {
  if (total <= limit) {
    return null;
  }

  const pageCount = Math.max(1, Math.ceil(total / limit));
  const prevPage = standingsPage > 1 ? standingsPage - 1 : null;
  const nextPage = standingsPage < pageCount ? standingsPage + 1 : null;
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
            render={<Link href={standingsPageHref(prevPage)} />}
          >
            Previous
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>
            Previous
          </Button>
        )}
        <span className="text-sm text-muted-foreground tabular-nums">
          Page {standingsPage} of {pageCount}
        </span>
        {nextPage ? (
          <Button
            variant="outline"
            size="sm"
            render={<Link href={standingsPageHref(nextPage)} />}
          >
            Next
          </Button>
        ) : (
          <Button variant="outline" size="sm" disabled>
            Next
          </Button>
        )}
      </div>
    </div>
  );
}
