import type { GameLeaderboardsResponse } from "@/generated/api-client";

type Props = {
  data: Pick<
    GameLeaderboardsResponse,
    "developer" | "publisher" | "genre" | "releasedAt"
  >;
};

export function GameMetadata({ data }: Props) {
  const rows = [
    { label: "Developer", value: data.developer },
    { label: "Publisher", value: data.publisher },
    { label: "Genre", value: data.genre },
    {
      label: "Released",
      value: data.releasedAt
        ? new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(
            new Date(data.releasedAt),
          )
        : null,
    },
  ].filter((row) => row.value);

  if (rows.length === 0) {
    return null;
  }

  return (
    <dl className="grid grid-cols-1 gap-x-6 gap-y-1 text-sm sm:grid-cols-2">
      {rows.map((row) => (
        <div key={row.label} className="flex gap-2">
          <dt className="text-muted-foreground">{row.label}</dt>
          <dd>{row.value}</dd>
        </div>
      ))}
    </dl>
  );
}
