import { AdminGameOfTheWeekSection } from "@/components/admin/admin-game-of-the-week-section";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminGameOfTheWeekPage() {
  let currentPoll = null;
  try {
    const api = await getServerApiClient();
    currentPoll = await api.getAdminGameOfTheWeekCurrentPoll();
  } catch {
    currentPoll = null;
  }

  return (
    <div className="space-y-6">
      <AdminGameOfTheWeekSection currentPoll={currentPoll} />
    </div>
  );
}
