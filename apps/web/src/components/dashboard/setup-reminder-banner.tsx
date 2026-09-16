import Link from "next/link";

export function SetupReminderBanner() {
  return (
    <p className="flex flex-wrap items-center gap-2 rounded border border-[var(--accent-retro)]/30 bg-[var(--accent-retro)]/10 px-4 py-3 text-sm">
      <span>Link your API key and run your first sync to appear on leaderboards.</span>
      <Link href="/settings" className="font-medium text-[var(--accent-retro)] hover:underline">
        Finish setup in Settings →
      </Link>
    </p>
  );
}
