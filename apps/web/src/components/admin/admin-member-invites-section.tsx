"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminMemberInviteDto } from "@/generated/api-client";
import { DataFieldList } from "@/components/layout/data-field-list";
import { ResponsiveTable } from "@/components/layout/responsive-table";
import { Button } from "@/components/ui/button";
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
        <Button type="submit" className="w-full sm:w-auto" disabled={pending}>
          {pending ? "Adding…" : "Add invite"}
        </Button>
      </form>

      {error && (
        <p className="rounded border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm">{error}</p>
      )}

      <ResponsiveTable
        rows={initialInvites}
        rowKey={(invite) => invite.discordId}
        emptyMessage={
          <p className="text-sm text-muted-foreground">No invites yet.</p>
        }
        columns={[
          {
            header: "Discord ID",
            cellClassName: "font-mono text-xs",
            render: (invite) => invite.discordId,
          },
          {
            header: "Label",
            render: (invite) => invite.displayName ?? "—",
          },
          {
            header: "RA linked",
            render: (invite) => (invite.hasRaAccount ? "Yes" : "Pending"),
          },
          {
            header: "API key",
            render: (invite) => (invite.hasApiKey ? "Yes" : "No"),
          },
          {
            header: "Admin",
            render: (invite) => (invite.isAdmin ? "Yes" : "No"),
          },
          {
            header: "",
            headerClassName: "w-[100px]",
            render: (invite) =>
              !invite.hasRaAccount ? (
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
              ) : null,
          },
        ]}
        renderMobileCard={(invite) => (
          <li key={invite.discordId} className="rounded border border-border bg-card p-4 text-sm">
            <p className="font-mono text-xs">{invite.discordId}</p>
            <p className="mt-1 font-medium">{invite.displayName ?? "—"}</p>
            <DataFieldList
              className="mt-2"
              fields={[
                { label: "RA linked", value: invite.hasRaAccount ? "Yes" : "Pending" },
                { label: "API key", value: invite.hasApiKey ? "Yes" : "No" },
                { label: "Admin", value: invite.isAdmin ? "Yes" : "No" },
              ]}
            />
            {!invite.hasRaAccount ? (
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="mt-3 w-full"
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
            ) : null}
          </li>
        )}
      />
    </section>
  );
}
