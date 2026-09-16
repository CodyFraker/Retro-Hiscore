import type { SyncHealthResponse } from "@/generated/api-client";

type Props = {
  health: SyncHealthResponse;
};

export function SyncHealthAlert({ health }: Props) {
  const messages = [
    health.groupMessage,
    ...health.memberIssues.map((issue) => issue.message),
  ].filter(Boolean);

  if (messages.length === 0) {
    return null;
  }

  const isRateLimited = health.groupStatus === "RateLimited";

  return (
    <div
      className={
        isRateLimited
          ? "rounded border border-amber-500/40 bg-amber-500/10 px-4 py-3 text-sm"
          : "rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm"
      }
    >
      <ul className="list-disc space-y-1 pl-4">
        {messages.map((message) => (
          <li key={message}>{message}</li>
        ))}
      </ul>
    </div>
  );
}
