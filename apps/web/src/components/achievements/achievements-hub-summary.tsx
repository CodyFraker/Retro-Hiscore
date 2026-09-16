import Link from "next/link";

type Props = {
  hasFilters: boolean;
  filteredTotal?: number;
  filterContext?: string;
};

export function AchievementsHubSummary({ hasFilters, filteredTotal, filterContext }: Props) {
  if (hasFilters) {
    return (
      <p className="text-sm text-muted-foreground">
        {filteredTotal != null ? (
          <>
            <span className="font-mono text-foreground">{filteredTotal}</span> matching unlock
            {filteredTotal === 1 ? "" : "s"}
            {filterContext ? ` · ${filterContext}` : ""}
            {" · "}
          </>
        ) : null}
        Group stats on the{" "}
        <Link href="/members" className="text-foreground underline-offset-4 hover:underline">
          Members hub
        </Link>
        .
      </p>
    );
  }

  return (
    <p className="text-sm text-muted-foreground">
      Group unlock stats ·{" "}
      <Link href="/members" className="text-foreground underline-offset-4 hover:underline">
        Members hub
      </Link>
    </p>
  );
}
