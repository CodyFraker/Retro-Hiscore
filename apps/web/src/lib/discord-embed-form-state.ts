export type DiscordEmbedFieldForm = {
  name: string;
  value: string;
};

export type DiscordEmbedFormState = {
  title: string;
  description: string;
  color: string;
  footer: string;
  thumbnailUrl: string;
  imageUrl: string;
  fields: DiscordEmbedFieldForm[];
};

export type DiscordWebhookFormState = {
  content: string;
  embed: DiscordEmbedFormState;
};

const DEFAULT_EMBED: DiscordEmbedFormState = {
  title: "",
  description: "",
  color: "5793266",
  footer: "",
  thumbnailUrl: "",
  imageUrl: "",
  fields: [],
};

export function parseWebhookPayloadJson(json: string): DiscordWebhookFormState {
  const parsed = JSON.parse(json) as {
    content?: string;
    embeds?: Array<{
      title?: string;
      description?: string;
      color?: number;
      footer?: { text?: string };
      thumbnail?: { url?: string };
      image?: { url?: string };
      fields?: Array<{ name?: string; value?: string }>;
    }>;
  };

  const embed = parsed.embeds?.[0] ?? {};
  return {
    content: parsed.content ?? "",
    embed: {
      title: embed.title ?? "",
      description: embed.description ?? "",
      color: embed.color != null ? String(embed.color) : DEFAULT_EMBED.color,
      footer: embed.footer?.text ?? "",
      thumbnailUrl: embed.thumbnail?.url ?? "",
      imageUrl: embed.image?.url ?? "",
      fields: (embed.fields ?? []).map((f) => ({
        name: f.name ?? "",
        value: f.value ?? "",
      })),
    },
  };
}

export function buildWebhookPayloadJson(state: DiscordWebhookFormState): string {
  const color = Number.parseInt(state.embed.color, 10);
  const embed: Record<string, unknown> = {
    title: state.embed.title || undefined,
    description: state.embed.description || undefined,
    color: Number.isFinite(color) ? color : undefined,
  };

  if (state.embed.footer) {
    embed.footer = { text: state.embed.footer };
  }
  if (state.embed.thumbnailUrl) {
    embed.thumbnail = { url: state.embed.thumbnailUrl };
  }
  if (state.embed.imageUrl) {
    embed.image = { url: state.embed.imageUrl };
  }
  if (state.embed.fields.length > 0) {
    embed.fields = state.embed.fields.map((f) => ({ name: f.name, value: f.value }));
  }

  const payload: Record<string, unknown> = { embeds: [embed] };
  if (state.content.trim()) {
    payload.content = state.content;
  }

  return JSON.stringify(payload, null, 2);
}

export function defaultFormStateFromTemplateJson(json: string): DiscordWebhookFormState {
  try {
    return parseWebhookPayloadJson(json);
  } catch {
    return { content: "", embed: { ...DEFAULT_EMBED } };
  }
}
