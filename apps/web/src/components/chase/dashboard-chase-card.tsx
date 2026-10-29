import Link from "next/link";
import { Target } from "lucide-react";
import { ChaseList } from "@/components/chase/chase-list";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { MemberChaseItemDto } from "@/generated/api-client";

type Props = {
  items: MemberChaseItemDto[];
};

export function DashboardChaseCard({ items }: Props) {
  if (items.length === 0) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Target className="size-4 shrink-0 text-[var(--accent-retro)]" />
          Chase list
        </CardTitle>
      </CardHeader>
      <CardContent>
        <ChaseList items={items} compact />
      </CardContent>
      {items.length > 5 ? (
        <CardFooter className="border-t py-3">
          <Link href="/chase" className="text-sm text-[var(--accent-retro)] hover:underline">
            View all {items.length} boards →
          </Link>
        </CardFooter>
      ) : (
        <CardFooter className="border-t py-3">
          <Link href="/chase" className="text-sm text-muted-foreground hover:text-[var(--accent-retro)]">
            Full chase list →
          </Link>
        </CardFooter>
      )}
    </Card>
  );
}
