"use client";

import type { DiscordWebhookFormState } from "@/lib/discord-embed-form-state";

type Props = {
  form: DiscordWebhookFormState;
};

function colorToHex(color: string) {
  const n = Number.parseInt(color, 10);
  if (!Number.isFinite(n)) {
    return "#5865f2";
  }
  return `#${(n & 0xffffff).toString(16).padStart(6, "0")}`;
}

export function DiscordEmbedPreview({ form }: Props) {
  const { embed, content } = form;
  const accent = colorToHex(embed.color);

  return (
    <div className="space-y-2 rounded-lg border border-border bg-[#313338] p-4 text-[#dbdee1]">
      <p className="text-xs font-medium uppercase tracking-wide text-[#949ba4]">Preview</p>
      {content ? <p className="text-sm whitespace-pre-wrap">{content}</p> : null}
      <div className="flex gap-0 overflow-hidden rounded-md bg-[#2b2d31]">
        <div className="w-1 shrink-0" style={{ backgroundColor: accent }} />
        <div className="min-w-0 flex-1 space-y-2 p-3">
          {embed.title ? (
            <p className="text-sm font-semibold text-[#f2f3f5]">{embed.title}</p>
          ) : null}
          {embed.description ? (
            <p className="text-sm whitespace-pre-wrap text-[#dbdee1]">{embed.description}</p>
          ) : null}
          {embed.fields.map((field, i) => (
            <div key={i} className="grid gap-0.5">
              <p className="text-xs font-semibold text-[#f2f3f5]">{field.name}</p>
              <p className="text-xs whitespace-pre-wrap">{field.value}</p>
            </div>
          ))}
          {embed.footer ? <p className="text-xs text-[#949ba4]">{embed.footer}</p> : null}
        </div>
      </div>
    </div>
  );
}
