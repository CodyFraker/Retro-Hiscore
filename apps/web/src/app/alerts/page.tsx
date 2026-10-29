import { PageHero } from "@/components/layout/page-hero";
import { AlertsList } from "@/components/alerts/alerts-list";
import { getServerApiClient } from "@/lib/api";
import { rethrowIfNavigationError } from "@/lib/invalid-session";

export const dynamic = "force-dynamic";

export default async function AlertsPage() {
  let items: Awaited<ReturnType<Awaited<ReturnType<typeof getServerApiClient>>["getMemberAlerts"]>>["items"] =
    [];
  let unreadCount = 0;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const response = await api.getMemberAlerts();
    items = response.items;
    unreadCount = response.unreadCount;
  } catch (err) {
    rethrowIfNavigationError(err);
    error = err instanceof Error ? err.message : "Failed to load alerts";
  }

  return (
    <div className="mx-auto max-w-2xl space-y-8">
      <PageHero title="Alerts" description="Personal updates when boards move or friends unlock achievements." />
      {error ? (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">{error}</p>
      ) : (
        <AlertsList items={items} unreadCount={unreadCount} />
      )}
    </div>
  );
}
