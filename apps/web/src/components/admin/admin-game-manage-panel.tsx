"use client";

import { ArrowLeft, RefreshCw, Trash2 } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { AdminGameDto, GameSourceDto, UpsertGameSourceRequest } from "@/generated/api-client";
import { Button } from "@/components/ui/button";
import {
  GAME_SOURCE_TYPE_OPTIONS,
  gameSourceTypeLabel,
} from "@/lib/game-source-labels";
import { LeaderboardSyncTierBadge } from "@/components/sync/leaderboard-sync-tier-badge";
import {
  deleteAdminGameAction,
  deleteAdminGameSourceAction,
  patchAdminGameLeaderboardSyncAction,
  postAdminGameRefreshAction,
  upsertAdminGameSourceAction,
} from "@/lib/actions/admin";

type Props = {
  game: AdminGameDto;
  initialSources: GameSourceDto[];
};

const emptyForm: UpsertGameSourceRequest = {
  sourceType: "GoogleDrive",
  url: "",
  label: null,
  sortOrder: 0,
  note: null,
};

export function AdminGameManagePanel({ game, initialSources }: Props) {
  const router = useRouter();
  const [sources, setSources] = useState(initialSources);
  const [form, setForm] = useState<UpsertGameSourceRequest>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [forceCold, setForceCold] = useState(game.forceColdLeaderboardSync);
  const [pending, startTransition] = useTransition();

  function resetForm() {
    setForm(emptyForm);
    setEditingId(null);
  }

  function startEdit(source: GameSourceDto) {
    setEditingId(source.id);
    setForm({
      sourceType: source.sourceType,
      url: source.url,
      label: source.label ?? null,
      sortOrder: source.sortOrder,
      note: source.note ?? null,
    });
  }

  return (
    <div className="space-y-10">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="space-y-2">
          <Link
            href="/admin/games"
            className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" />
            Games
          </Link>
          <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
            {game.title}
          </h1>
          <p className="text-sm text-muted-foreground">
            RA #{game.raGameId} · {game.leaderboardCount} boards · {game.sourceCount} mirrors
          </p>
          <Link href={`/games/${game.raGameId}`} className="text-sm text-[var(--accent-retro)] hover:underline">
            Open public game page
          </Link>
        </div>
        <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row sm:flex-wrap">
          <Button
            type="button"
            variant="outline"
            className="w-full sm:w-auto"
            disabled={pending}
            onClick={() => {
              startTransition(async () => {
                setError(null);
                const result = await postAdminGameRefreshAction(game.raGameId);
                if (!result.ok) {
                  setError(result.error);
                  return;
                }
                router.refresh();
              });
            }}
          >
            <RefreshCw className="size-4" />
            Refresh metadata &amp; scores
          </Button>
          <Button
            type="button"
            variant="destructive"
            className="w-full sm:w-auto"
            disabled={pending}
            onClick={() => {
              if (!window.confirm(`Stop tracking ${game.title}? This removes leaderboard data and mirrors.`)) {
                return;
              }
              startTransition(async () => {
                setError(null);
                const result = await deleteAdminGameAction(game.raGameId);
                if (!result.ok) {
                  setError(result.error);
                  return;
                }
                router.push("/admin/games");
                router.refresh();
              });
            }}
          >
            <Trash2 className="size-4" />
            Stop tracking
          </Button>
        </div>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <section className="space-y-3 rounded border border-border p-5">
        <h2 className="text-lg font-semibold">Leaderboard sync</h2>
        <p className="text-sm text-muted-foreground">
          Hot games sync more often when the group has played recently. Pin to cold to reduce RetroAchievements API
          calls; manual refresh still works.
        </p>
        <div className="flex flex-wrap items-center gap-3">
          <LeaderboardSyncTierBadge status={game.leaderboardSyncStatus} />
          <label className="flex cursor-pointer items-center gap-2 text-sm">
            <input
              type="checkbox"
              className="size-4 rounded border-border"
              checked={forceCold}
              disabled={pending}
              onChange={(event) => {
                const next = event.target.checked;
                startTransition(async () => {
                  setError(null);
                  const result = await patchAdminGameLeaderboardSyncAction(game.raGameId, next);
                  if (!result.ok) {
                    setError(result.error);
                    return;
                  }
                  setForceCold(result.game.forceColdLeaderboardSync);
                  router.refresh();
                });
              }}
            />
            Force cold schedule
          </label>
        </div>
      </section>

      <section className="space-y-4 rounded border border-border p-5">
        <h2 className="text-lg font-semibold">Download mirrors</h2>

        {sources.length === 0 ? (
          <p className="text-sm text-muted-foreground">No mirrors yet. Add one below.</p>
        ) : (
          <ul className="divide-y divide-border rounded border border-border">
            {sources.map((source) => (
              <li
                key={source.id}
                className="flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between"
              >
                <div className="min-w-0">
                  <p className="font-medium">
                    {gameSourceTypeLabel(source.sourceType, source.label)}
                    <span className="ml-2 text-xs text-muted-foreground">order {source.sortOrder}</span>
                  </p>
                  <p className="truncate text-sm text-muted-foreground">{source.url}</p>
                  {source.note && <p className="text-sm text-muted-foreground">{source.note}</p>}
                </div>
                <div className="flex shrink-0 gap-2">
                  <Button type="button" size="sm" variant="outline" onClick={() => startEdit(source)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    size="sm"
                    variant="ghost"
                    disabled={pending}
                    onClick={() => {
                      startTransition(async () => {
                        setError(null);
                        const result = await deleteAdminGameSourceAction(game.raGameId, source.id);
                        if (!result.ok) {
                          setError(result.error);
                          return;
                        }
                        setSources((prev) => prev.filter((s) => s.id !== source.id));
                        if (editingId === source.id) {
                          resetForm();
                        }
                        router.refresh();
                      });
                    }}
                  >
                    Remove
                  </Button>
                </div>
              </li>
            ))}
          </ul>
        )}

        <form
          className="grid gap-3 sm:grid-cols-2"
          onSubmit={(event) => {
            event.preventDefault();
            startTransition(async () => {
              setError(null);
              const result = await upsertAdminGameSourceAction(game.raGameId, editingId, form);
              if (!result.ok) {
                setError(result.error);
                return;
              }
              const saved = result.source;
              if (editingId) {
                setSources((prev) =>
                  prev
                    .map((s) => (s.id === editingId ? saved : s))
                    .sort((a, b) => a.sortOrder - b.sortOrder),
                );
              } else {
                setSources((prev) => [...prev, saved].sort((a, b) => a.sortOrder - b.sortOrder));
              }
              resetForm();
              router.refresh();
            });
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Type</span>
            <select
              value={form.sourceType}
              disabled={pending}
              onChange={(event) => setForm((f) => ({ ...f, sourceType: event.target.value }))}
              className="h-9 rounded-md bg-input px-2.5 outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            >
              {GAME_SOURCE_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-muted-foreground">URL (https)</span>
            <input
              type="url"
              required
              value={form.url}
              disabled={pending}
              onChange={(event) => setForm((f) => ({ ...f, url: event.target.value }))}
              className="h-9 rounded-md bg-input px-2.5 outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Label (optional)</span>
            <input
              type="text"
              value={form.label ?? ""}
              disabled={pending}
              onChange={(event) => setForm((f) => ({ ...f, label: event.target.value }))}
              placeholder="e.g. Archive.org"
              className="h-9 rounded-md bg-input px-2.5 outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Sort order</span>
            <input
              type="number"
              value={form.sortOrder}
              disabled={pending}
              onChange={(event) =>
                setForm((f) => ({ ...f, sortOrder: Number(event.target.value) || 0 }))
              }
              className="h-9 rounded-md bg-input px-2.5 outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-muted-foreground">Note (optional)</span>
            <input
              type="text"
              value={form.note ?? ""}
              disabled={pending}
              onChange={(event) => setForm((f) => ({ ...f, note: event.target.value }))}
              className="h-9 rounded-md bg-input px-2.5 outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            />
          </label>
          <div className="flex gap-2 sm:col-span-2">
            <Button type="submit" disabled={pending}>
              {editingId ? "Save mirror" : "Add mirror"}
            </Button>
            {editingId && (
              <Button type="button" variant="outline" disabled={pending} onClick={resetForm}>
                Cancel edit
              </Button>
            )}
          </div>
        </form>
      </section>
    </div>
  );
}
