import { AdminMemberInvitesSection } from "@/components/admin/admin-member-invites-section";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminMembersPage() {
  let invites;
  try {
    const api = await getServerApiClient();
    invites = await api.getAdminMemberInvites();
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load member invites";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load members"}
        </h1>
        <p className="text-muted-foreground">
          {forbidden ? "Your account is not an administrator on the API." : message}
        </p>
      </div>
    );
  }

  return (
    <section className="rounded border border-border p-5">
      <AdminMemberInvitesSection initialInvites={invites} />
    </section>
  );
}
