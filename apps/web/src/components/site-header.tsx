import { getServerSession } from "next-auth";
import { SiteHeaderBar } from "@/components/site-header-bar";
import { authOptions } from "@/lib/auth-options";
import { formatSyncTime } from "@/lib/format";
import { getServerApiClient } from "@/lib/api";

export async function SiteHeader() {
  const session = await getServerSession(authOptions);
  if (!session) {
    return null;
  }

  let syncLabel = "Never synced";
  let metadataLabel = "Game art: never synced";
  try {
    const api = await getServerApiClient();
    const [status, metadataStatus] = await Promise.all([
      api.getSyncStatus(),
      api.getMetadataSyncStatus(),
    ]);
    syncLabel = `Last sync: ${formatSyncTime(status.finishedAt ?? status.startedAt)}`;
    if (status.status) {
      syncLabel += ` (${status.status})`;
    }
    metadataLabel = `Game art: ${formatSyncTime(metadataStatus.finishedAt ?? metadataStatus.startedAt)}`;
    if (metadataStatus.status) {
      metadataLabel += ` (${metadataStatus.status})`;
    }
  } catch {
    syncLabel = "API unreachable";
    metadataLabel = "";
  }

  const displayName = session.user.discordUsername ?? session.user.name ?? "Signed in";

  return (
    <SiteHeaderBar
      displayName={displayName}
      avatarUrl={session.user.image}
      syncLabel={syncLabel}
      metadataLabel={metadataLabel}
    />
  );
}
