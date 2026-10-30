import Link from "next/link";
import { notFound } from "next/navigation";
import { PageHero } from "@/components/layout/page-hero";
import { MonthSummaryContent } from "@/components/summary/month-summary-content";
import { MonthSummaryMonthPicker } from "@/components/summary/month-summary-month-picker";
import { getServerApiClient } from "@/lib/api";
import { rethrowIfNavigationError } from "@/lib/invalid-session";

export const dynamic = "force-dynamic";

type Props = {
  params: Promise<{ year: string; month: string }>;
};

function parseYearMonth(yearRaw: string, monthRaw: string) {
  const year = Number(yearRaw);
  const month = Number(monthRaw);
  if (!Number.isInteger(year) || !Number.isInteger(month) || month < 1 || month > 12 || year < 2000 || year > 2100) {
    return null;
  }
  return { year, month };
}

export default async function SummaryMonthPage({ params }: Props) {
  const { year: yearRaw, month: monthRaw } = await params;
  const parsed = parseYearMonth(yearRaw, monthRaw);
  if (parsed == null) {
    notFound();
  }

  const { year, month } = parsed;

  try {
    const api = await getServerApiClient();
    const summary = await api.getDashboardMonthSummary(year, month);

    return (
      <div className="mx-auto max-w-2xl space-y-8">
        <PageHero
          title="Month summary"
          description="Games played, achievements, and leaderboard activity for your group in the selected UTC month."
          actions={
            <MonthSummaryMonthPicker
              year={summary.year}
              month={summary.month}
              availableMonths={summary.availableMonths}
            />
          }
        />
        <MonthSummaryContent summary={summary} />
        <p className="text-sm text-muted-foreground">
          <Link href="/" className="hover:text-[var(--accent-retro)]">
            Back to dashboard
          </Link>
        </p>
      </div>
    );
  } catch (error) {
    rethrowIfNavigationError(error);
    throw error;
  }
}
