type Props = {
  startsAt: string;
  endsAt: string;
  gameLabels: string[];
  siteUrl?: string;
};

export function AdminGameOfTheWeekDiscordPreview({
  startsAt,
  endsAt,
  gameLabels,
  siteUrl = "https://yoursite.example",
}: Props) {
  const startLabel = startsAt ? new Date(startsAt).toLocaleString() : "—";
  const endLabel = endsAt ? new Date(endsAt).toLocaleString() : "—";
  const ballotLines =
    gameLabels.length > 0
      ? gameLabels.map((label, i) => `${i + 1}. ${label}`).join("\n")
      : "Add seed games to preview the ballot.";

  return (
    <div className="space-y-2">
      <p className="text-sm font-medium">Discord announcement preview</p>
      <div className="rounded-lg border border-border bg-[#313338] p-4 text-[#dbdee1]">
        <div className="flex gap-0 overflow-hidden rounded-md bg-[#2b2d31]">
          <div className="w-1 shrink-0 bg-[#57a64a]" />
          <div className="min-w-0 flex-1 space-y-2 p-3">
            <p className="text-sm font-semibold text-[#f2f3f5]">Game of the week — vote open</p>
            <p className="text-sm whitespace-pre-wrap text-[#dbdee1]">
              {`Voting runs ${startLabel} through ${endLabel}.\n\n${ballotLines}\n\nVote: ${siteUrl}/game-of-the-week`}
            </p>
          </div>
        </div>
      </div>
      <p className="text-xs text-muted-foreground">Preview only — post manually or configure a webhook later.</p>
    </div>
  );
}
