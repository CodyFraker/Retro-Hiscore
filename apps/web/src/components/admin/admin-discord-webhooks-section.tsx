"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";
import type {
  AdminDiscordWebhookDetailDto,
  AdminDiscordWebhookSummaryDto,
  AdminNotificationDispatchRunDto,
  DiscordTokenCatalogDto,
} from "@/generated/api-client";
import { DiscordEmbedPreview } from "@/components/admin/discord-embed-preview";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  buildWebhookPayloadJson,
  defaultFormStateFromTemplateJson,
  parseWebhookPayloadJson,
  type DiscordWebhookFormState,
} from "@/lib/discord-embed-form-state";
import {
  defaultPayloadTemplateForEvent,
  DISCORD_EVENT_KIND_OPTIONS,
} from "@/lib/discord-webhook-default-templates";
import {
  deleteAdminDiscordWebhookAction,
  getAdminDiscordWebhookAction,
  postAdminDiscordWebhookTestAction,
  saveAdminDiscordWebhookAction,
} from "@/lib/actions/admin";

type Props = {
  initialWebhooks: AdminDiscordWebhookSummaryDto[];
  tokenCatalog: DiscordTokenCatalogDto;
  dispatchRuns: AdminNotificationDispatchRunDto[];
};

type EditorMode = { type: "create" } | { type: "edit"; id: string };

function tokensForKinds(
  catalog: DiscordTokenCatalogDto,
  kinds: string[],
): { code: string; label: string }[] {
  const map = new Map<string, string>();
  for (const kind of kinds) {
    const entry = catalog.events.find((e) => e.eventKind === kind);
    for (const t of entry?.tokens ?? []) {
      map.set(t.code, t.label);
    }
  }
  return [...map.entries()].map(([code, label]) => ({ code, label })).sort((a, b) => a.code.localeCompare(b.code));
}

export function AdminDiscordWebhooksSection({ initialWebhooks, tokenCatalog, dispatchRuns }: Props) {
  const router = useRouter();
  const [editor, setEditor] = useState<EditorMode | null>(null);
  const [name, setName] = useState("");
  const [webhookUrl, setWebhookUrl] = useState("");
  const [enabled, setEnabled] = useState(true);
  const [digestMinutes, setDigestMinutes] = useState("10");
  const [eventKinds, setEventKinds] = useState<string[]>(["LeaderboardFriendOvertake"]);
  const [jsonTab, setJsonTab] = useState(false);
  const [payloadJson, setPayloadJson] = useState(defaultPayloadTemplateForEvent("LeaderboardFriendOvertake"));
  const [form, setForm] = useState<DiscordWebhookFormState>(() =>
    defaultFormStateFromTemplateJson(defaultPayloadTemplateForEvent("LeaderboardFriendOvertake")),
  );
  const [error, setError] = useState<string | null>(null);
  const [previewJson, setPreviewJson] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const palette = useMemo(() => tokensForKinds(tokenCatalog, eventKinds), [tokenCatalog, eventKinds]);

  function openCreate() {
    setEditor({ type: "create" });
    setName("");
    setWebhookUrl("");
    setEnabled(true);
    setDigestMinutes("10");
    setEventKinds(["LeaderboardFriendOvertake"]);
    const template = defaultPayloadTemplateForEvent("LeaderboardFriendOvertake");
    setPayloadJson(template);
    setForm(defaultFormStateFromTemplateJson(template));
    setError(null);
    setPreviewJson(null);
  }

  async function openEdit(id: string) {
    setError(null);
    setPreviewJson(null);
    setEditor({ type: "edit", id });
    const loaded = await getAdminDiscordWebhookAction(id);
    if (!loaded.ok) {
      setError(loaded.error);
      return;
    }
    const detail = loaded.webhook;
    setName(detail.name);
    setWebhookUrl("");
    setEnabled(detail.enabled);
    setDigestMinutes(String(detail.digestIntervalMinutes));
    setEventKinds(detail.eventKinds);
    setPayloadJson(detail.payloadTemplateJson);
    setForm(defaultFormStateFromTemplateJson(detail.payloadTemplateJson));
  }

  function syncJsonFromForm() {
    setPayloadJson(buildWebhookPayloadJson(form));
  }

  function syncFormFromJson() {
    try {
      setForm(parseWebhookPayloadJson(payloadJson));
      setError(null);
    } catch {
      setError("Invalid embed JSON.");
    }
  }

  function insertToken(code: string) {
    const token = `{{${code}}}`;
    setForm((prev) => ({
      ...prev,
      embed: { ...prev.embed, description: `${prev.embed.description}${token}` },
    }));
    if (!jsonTab) {
      syncJsonFromForm();
    }
  }

  function toggleKind(kind: string) {
    setEventKinds((prev) =>
      prev.includes(kind) ? prev.filter((k) => k !== kind) : [...prev, kind],
    );
  }

  function handleSave() {
    startTransition(async () => {
      setError(null);
      const template = jsonTab ? payloadJson : buildWebhookPayloadJson(form);
      const digest = Number.parseInt(digestMinutes, 10);
      const result = await saveAdminDiscordWebhookAction({
        id: editor?.type === "edit" ? editor.id : null,
        name,
        enabled,
        webhookUrl,
        digestIntervalMinutes: Number.isFinite(digest) ? digest : 10,
        payloadTemplateJson: template,
        eventKinds,
      });
      if (!result.ok) {
        setError(result.error);
        return;
      }
      setEditor(null);
      router.refresh();
    });
  }

  function handleTest() {
    if (editor?.type !== "edit") {
      return;
    }
    const kind = eventKinds[0] ?? "LeaderboardFriendOvertake";
    startTransition(async () => {
      const result = await postAdminDiscordWebhookTestAction(editor.id, kind);
      if (!result.ok) {
        setError(result.error);
      }
    });
  }

  function handleDelete(id: string) {
    startTransition(async () => {
      const result = await deleteAdminDiscordWebhookAction(id);
      if (!result.ok) {
        setError(result.error);
        return;
      }
      router.refresh();
    });
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-row flex-wrap items-center justify-between gap-4">
          <div>
            <CardTitle>Discord webhooks</CardTitle>
            <CardDescription>
              Configure embed templates with short tokens. Events enqueue after sync; dispatch runs every few minutes.
            </CardDescription>
          </div>
          <Button type="button" onClick={openCreate}>New webhook</Button>
        </CardHeader>
        <CardContent>
          {initialWebhooks.length === 0 ? (
            <p className="text-sm text-muted-foreground">No webhooks configured yet.</p>
          ) : (
            <ul className="divide-y divide-border">
              {initialWebhooks.map((w) => (
                <li key={w.id} className="flex flex-wrap items-center justify-between gap-2 py-3">
                  <div>
                    <p className="font-medium">{w.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {w.enabled ? "Enabled" : "Disabled"} · {w.digestIntervalMinutes}m digest ·{" "}
                      {w.eventKinds.join(", ")}
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <Button type="button" variant="outline" size="sm" onClick={() => openEdit(w.id)}>
                      Edit
                    </Button>
                    <Button type="button" variant="ghost" size="sm" onClick={() => handleDelete(w.id)}>
                      Delete
                    </Button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>

      {dispatchRuns.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Recent dispatch runs</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            {dispatchRuns.map((run) => (
              <div key={run.id} className="flex flex-wrap justify-between gap-2 border-b border-border pb-2">
                <span>{run.status}</span>
                <span className="text-muted-foreground">
                  {run.postsSucceeded}/{run.postsFailed} failed · {run.eventsProcessed} events
                </span>
                {run.error ? <span className="w-full text-destructive">{run.error}</span> : null}
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}

      {editor ? (
        <Card>
          <CardHeader>
            <CardTitle>{editor.type === "create" ? "Create webhook" : "Edit webhook"}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {error ? <p className="text-sm text-destructive">{error}</p> : null}
            <div className="grid gap-4 md:grid-cols-2">
              <label className="space-y-1 text-sm">
                <span>Name</span>
                <input
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
              </label>
              <label className="space-y-1 text-sm">
                <span>Webhook URL{editor.type === "edit" ? " (leave blank to keep)" : ""}</span>
                <input
                  type="password"
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  value={webhookUrl}
                  onChange={(e) => setWebhookUrl(e.target.value)}
                />
              </label>
              <label className="space-y-1 text-sm">
                <span>Digest interval (minutes)</span>
                <input
                  className="w-full rounded-md border border-input bg-background px-3 py-2"
                  value={digestMinutes}
                  onChange={(e) => setDigestMinutes(e.target.value)}
                />
              </label>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={enabled} onChange={(e) => setEnabled(e.target.checked)} />
                Enabled
              </label>
            </div>

            <div className="space-y-2">
              <p className="text-sm font-medium">Events</p>
              <div className="flex flex-wrap gap-3">
                {DISCORD_EVENT_KIND_OPTIONS.map((opt) => (
                  <label key={opt.value} className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={eventKinds.includes(opt.value)}
                      onChange={() => toggleKind(opt.value)}
                    />
                    {opt.label}
                  </label>
                ))}
              </div>
            </div>

            <div className="space-y-2">
              <p className="text-sm font-medium">Tokens</p>
              <div className="flex flex-wrap gap-2">
                {palette.map((t) => (
                  <button
                    key={t.code}
                    type="button"
                    title={t.label}
                    className="rounded border border-border px-2 py-1 text-xs font-mono hover:bg-muted"
                    onClick={() => insertToken(t.code)}
                  >
                    {`{{${t.code}}}`}
                  </button>
                ))}
              </div>
            </div>

            <div className="flex gap-2">
              <Button type="button" variant={jsonTab ? "outline" : "default"} size="sm" onClick={() => { setJsonTab(false); syncFormFromJson(); }}>
                Form
              </Button>
              <Button type="button" variant={jsonTab ? "default" : "outline"} size="sm" onClick={() => { syncJsonFromForm(); setJsonTab(true); }}>
                JSON
              </Button>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
              {jsonTab ? (
                <textarea
                  className="min-h-[280px] w-full rounded-md border border-input bg-background p-3 font-mono text-xs"
                  value={payloadJson}
                  onChange={(e) => setPayloadJson(e.target.value)}
                />
              ) : (
                <div className="space-y-3">
                  <input
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    placeholder="Embed title"
                    value={form.embed.title}
                    onChange={(e) => setForm((f) => ({ ...f, embed: { ...f.embed, title: e.target.value } }))}
                  />
                  <textarea
                    className="min-h-[120px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    placeholder="Description (Discord markdown)"
                    value={form.embed.description}
                    onChange={(e) => setForm((f) => ({ ...f, embed: { ...f.embed, description: e.target.value } }))}
                  />
                  <input
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    placeholder="Color (decimal)"
                    value={form.embed.color}
                    onChange={(e) => setForm((f) => ({ ...f, embed: { ...f.embed, color: e.target.value } }))}
                  />
                  <input
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    placeholder="Footer"
                    value={form.embed.footer}
                    onChange={(e) => setForm((f) => ({ ...f, embed: { ...f.embed, footer: e.target.value } }))}
                  />
                </div>
              )}
              <DiscordEmbedPreview form={jsonTab ? defaultFormStateFromTemplateJson(payloadJson) : form} />
            </div>

            {previewJson ? (
              <pre className="max-h-48 overflow-auto rounded-md bg-muted p-3 text-xs">{previewJson}</pre>
            ) : null}

            <div className="flex flex-wrap gap-2">
              <Button type="button" disabled={pending} onClick={handleSave}>
                Save
              </Button>
              {editor.type === "edit" ? (
                <Button type="button" variant="outline" disabled={pending} onClick={handleTest}>
                  Send test
                </Button>
              ) : null}
              <Button type="button" variant="ghost" onClick={() => setEditor(null)}>
                Cancel
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}
