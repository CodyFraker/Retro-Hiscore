"use client";

import Link from "next/link";
import type { AdminGameDto } from "@/generated/api-client";
import { ConsoleName } from "@/components/console-name";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

type Props = {
  games: AdminGameDto[];
};

export function AdminGamesSection({ games }: Props) {
  return (
    <section className="space-y-4">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Title</TableHead>
            <TableHead className="hidden sm:table-cell">Platform</TableHead>
            <TableHead className="text-right">Mirrors</TableHead>
            <TableHead className="hidden md:table-cell">Metadata synced</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {games.map((game) => (
            <TableRow key={game.id}>
              <TableCell className="font-medium">
                <Link
                  href={`/admin/games/${game.raGameId}`}
                  className="hover:text-[var(--accent-retro)]"
                >
                  {game.title}
                </Link>
              </TableCell>
              <TableCell className="hidden sm:table-cell">
                <ConsoleName
                  name={game.consoleName}
                  iconUrl={game.consoleIconUrl}
                  fallback={`RA #${game.raGameId}`}
                />
              </TableCell>
              <TableCell className="text-right tabular-nums">{game.sourceCount}</TableCell>
              <TableCell className="hidden text-sm text-muted-foreground md:table-cell">
                {game.metadataSyncedAt ? (
                  <FormattedSyncTime value={game.metadataSyncedAt} />
                ) : (
                  "—"
                )}
              </TableCell>
              <TableCell className="text-right text-sm">
                <Link
                  href={`/games/${game.raGameId}`}
                  className="text-muted-foreground hover:text-foreground"
                >
                  View
                </Link>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </section>
  );
}
