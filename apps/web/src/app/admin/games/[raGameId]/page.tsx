import { notFound } from "next/navigation";
import { AdminGameManagePanel } from "@/components/admin/admin-game-manage-panel";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ raGameId: string }>;
};

export default async function AdminGameDetailPage({ params }: Props) {
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
