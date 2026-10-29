import { PageHero } from "@/components/layout/page-hero";
import { ChaseList } from "@/components/chase/chase-list";
import { getServerApiClient } from "@/lib/api";
import { rethrowIfNavigationError } from "@/lib/invalid-session";

export const dynamic = "force-dynamic";

export default async function ChasePage() {
  let items: Awaited<ReturnType<Awaited<ReturnType<typeof getServerApiClient>>["getMemberChase"]>>["items"] =
    [];
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    const response = await api.getMemberChase();
    items = response.items;
  } catch (err) {
    rethrowIfNavigationError(err);
    error = err instanceof Error ? err.message : "Failed to load chase list";
  }

  return (
    <div className="mx-auto max-w-2xl space-y-8">
      <PageHero
        title="Chase list"
        description="Boards where you hold friend rank 2 and the gap to the leader."
      />
      {error ? (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">{error}</p>
      ) : (
        <section className="rounded border border-border p-5">
          <ChaseList items={items} />
        </section>
      )}
    </div>
  );
}
