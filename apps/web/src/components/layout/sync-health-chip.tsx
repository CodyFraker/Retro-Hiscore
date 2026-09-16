import Link from "next/link";
import type { SyncHealthResponse } from "@/generated/api-client";

type Props = {
  health: SyncHealthResponse;
  admin?: boolean;
};

export function SyncHealthChip({ health, admin = false }: Props) {
  if (health.groupStatus === "Healthy" && health.memberIssues.length === 0) {
    return null;
  }

  const label =
    health.groupStatus === "RateLimited"
      ? "Sync: rate limited"
      : health.groupStatus === "Degraded"
        ? "Sync: delayed"
        : "Sync issue";

  const href = admin ? "/admin/sync" : "/settings";

  return (
    <Link
      href={href}
      className="rounded-full border border-amber-500/50 bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-200 hover:bg-amber-500/20"
    >
      {label}
    </Link>
  );
}
