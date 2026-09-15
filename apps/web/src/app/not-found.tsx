import type { Metadata } from "next";
import { Gamepad2 } from "lucide-react";
import Link from "next/link";
import { PageHero } from "@/components/layout/page-hero";

export const metadata: Metadata = {
  title: "Page not found",
  description: "This page does not exist on Retro Hiscore.",
};

export default function NotFound() {
  return (
    <div className="space-y-8">
      <PageHero
        title="Page not found"
        description="The link may be wrong, or this game or member is not available on your group's site."
      />

      <div className="flex flex-col items-start gap-4 sm:flex-row sm:items-center">
        <Gamepad2 className="size-10 text-muted-foreground" aria-hidden />
        <div className="flex flex-wrap gap-3">
          <Link
            href="/"
            className="inline-flex h-9 items-center rounded-md bg-secondary px-4 text-sm font-medium hover:bg-secondary/80"
          >
            Friend boards
          </Link>
          <Link
            href="/games"
            className="inline-flex h-9 items-center rounded-md border border-border px-4 text-sm font-medium hover:bg-secondary/40"
          >
            Tracked games
          </Link>
        </div>
      </div>
    </div>
  );
}
