import { AdminDiscordWebhooksSection } from "@/components/admin/admin-discord-webhooks-section";
import type {
  AdminDiscordWebhookSummaryDto,
  AdminNotificationDispatchRunDto,
  DiscordTokenCatalogDto,
} from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminWebhooksPage() {
  let webhooks: AdminDiscordWebhookSummaryDto[] = [];
  let tokenCatalog: DiscordTokenCatalogDto | null = null;
  let dispatchRuns: AdminNotificationDispatchRunDto[] = [];
  let loadError: string | null = null;
  let forbidden = false;

  try {
    const api = await getServerApiClient();
    const [webhooksResult, tokenCatalogResult, dispatchRunsResult] = await Promise.all([
      api.getAdminDiscordWebhooks(),
      api.getAdminDiscordWebhookTokenCatalog(),
      api.getAdminDiscordWebhookDispatchRuns(15),
    ]);
    webhooks = webhooksResult;
    tokenCatalog = tokenCatalogResult;
    dispatchRuns = dispatchRunsResult;
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load webhooks";
    forbidden = message.includes("403");
    loadError = message;
  }

  if (loadError) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load webhooks"}
        </h1>
        <p className="text-muted-foreground">{loadError}</p>
      </div>
    );
  }

  if (!tokenCatalog) {
    return null;
  }

  return (
    <AdminDiscordWebhooksSection
      initialWebhooks={webhooks}
      tokenCatalog={tokenCatalog}
      dispatchRuns={dispatchRuns}
    />
  );
}
