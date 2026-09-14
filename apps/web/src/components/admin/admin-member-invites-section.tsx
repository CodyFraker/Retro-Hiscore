"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminMemberInviteDto } from "@/generated/api-client";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  deleteAdminMemberInviteAction,
  postAdminMemberInviteAction,
} from "@/lib/actions/admin";

type Props = {
  initialInvites: AdminMemberInviteDto[];
};

export function AdminMemberInvitesSection({ initialInvites }: Props) {
  const router = useRouter();
  const [discordId, setDiscordId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <section className="space-y-4">
      <div>
        <h2 className="text-lg font-semibold">Discord invites</h2>
        <p className="text-sm text-muted-foreground">
          Add a Discord user ID to allow sign-in. They complete RetroAchievements setup in Settings after
          logging in.
        </p>
      </div>

      <form
        className="flex flex-col gap-3 sm:flex-row sm:items-end"
        onSubmit={(event) => {
          event.preventDefault();

          startTransition(async () => {
            setError(null);
            const result = await postAdminMemberInviteAction(discordId);
            if (!result.ok) {
              setError(result.error);
              return;
            }
            setDiscordId("");
            router.refresh();
          });
        }}
      >
        <div className="flex-1 space-y-2">
          <label htmlFor="discord-invite-id" className="text-sm font-medium">
            Discord user ID
          </label>
          <input
            id="discord-invite-id"
            type="text"
            inputMode="numeric"
            value={discordId}
            onChange={(event) => setDiscordId(event.target.value)}
            placeholder="e.g. 103967428408512512"
            className="w-full rounded-md bg-input px-3 py-2 font-mono text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
          />
        </div>
        <Button type="submit" disabled={pending}>
          {pending ? "Adding…" : "Add invite"}
        </Button>
      </form>

      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm">{error}</p>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Discord ID</TableHead>
            <TableHead>Label</TableHead>
            <TableHead>RA linked</TableHead>
            <TableHead>API key</TableHead>
            <TableHead>Admin</TableHead>
            <TableHead className="w-[100px]" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {initialInvites.length === 0 ? (
            <TableRow>
              <TableCell colSpan={6} className="text-muted-foreground">
                No invites yet.
              </TableCell>
            </TableRow>
          ) : (
            initialInvites.map((invite) => (
              <TableRow key={invite.discordId}>
                <TableCell className="font-mono text-xs">{invite.discordId}</TableCell>
                <TableCell>{invite.displayName ?? "—"}</TableCell>
                <TableCell>{invite.hasRaAccount ? "Yes" : "Pending"}</TableCell>
                <TableCell>{invite.hasApiKey ? "Yes" : "No"}</TableCell>
                <TableCell>{invite.isAdmin ? "Yes" : "No"}</TableCell>
                <TableCell>
                  {!invite.hasRaAccount && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      disabled={pending}
                      onClick={() => {
                        startTransition(async () => {
                          setError(null);
                          const result = await deleteAdminMemberInviteAction(invite.discordId);
                          if (!result.ok) {
                            setError(result.error);
                            return;
                          }
                          router.refresh();
                        });
                      }}
                    >
                      Remove
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </section>
  );
}
