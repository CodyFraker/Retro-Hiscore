import { buildWebhookPayloadJson, parseWebhookPayloadJson } from "@/lib/discord-embed-form-state";

describe("discord-embed-form-state", () => {
  it("roundtrips embed form through JSON", () => {
    // Arrange
    const json = `{
  "embeds": [{
    "title": "{{usr}}",
    "description": "Hello",
    "color": 5793266
  }]
}`;

    // Act
    const form = parseWebhookPayloadJson(json);
    const rebuilt = buildWebhookPayloadJson(form);
    const again = parseWebhookPayloadJson(rebuilt);

    // Assert
    expect(again.embed.title).toBe("{{usr}}");
    expect(again.embed.description).toBe("Hello");
    expect(again.embed.color).toBe("5793266");
  });
});
