import { getServerSession } from "next-auth";
import { AdminTabNav } from "@/components/admin/admin-tab-nav";
import { PageHero } from "@/components/layout/page-hero";
import { authOptions } from "@/lib/auth-options";

export const dynamic = "force-dynamic";

export default async function AdminLayout({ children }: LayoutProps<"/admin">) {
  const session = await getServerSession(authOptions);

  if (!session?.isAdmin) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          Not authorized
        </h1>
        <p className="text-muted-foreground">This page is only available to site administrators.</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <PageHero
        title="Admin"
        description="Manage members, sync schedules, and tracked games for your group."
      />
      <AdminTabNav />
      {children}
    </div>
  );
}
