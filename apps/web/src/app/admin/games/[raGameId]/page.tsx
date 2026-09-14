import { getServerSession } from "next-auth";
import { notFound } from "next/navigation";
import { AdminGameManagePanel } from "@/components/admin/admin-game-manage-panel";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raGameId: string }>;
};

export default async function AdminGameDetailPage({ params }: Props) {
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

  const { raGameId: raw } = await params;
  const raGameId = Number(raw);
  if (!Number.isFinite(raGameId)) {
    notFound();
  }

  let game;
  let sources;
  try {
    const api = await getServerApiClient();
    const games = await api.getAdminGames();
    game = games.find((g) => g.raGameId === raGameId);
    if (!game) {
      notFound();
    }

    sources = await api.getAdminGameSources(raGameId);
  } catch {
    notFound();
  }

  return <AdminGameManagePanel game={game} initialSources={sources} />;
}
