import { getApiBaseUrl } from "@/lib/api-base-url";

describe("getApiBaseUrl", () => {
  const originalWindow = global.window;

  afterEach(() => {
    if (originalWindow === undefined) {
      // @ts-expect-error test cleanup
      delete global.window;
    } else {
      global.window = originalWindow;
    }
    delete process.env.API_URL;
  });

  it("throws in the browser", () => {
    // Arrange
    // @ts-expect-error jsdom-style window stub
    global.window = {};

    // Act & Assert
    expect(() => getApiBaseUrl()).toThrow("must only be called on the server");
  });

  it("returns API_URL on the server when set", () => {
    // Arrange
    process.env.API_URL = "http://api:8080";

    // Act
    const base = getApiBaseUrl();

    // Assert
    expect(base).toBe("http://api:8080");
  });

  it("falls back to local API port on the server when API_URL is unset", () => {
    // Arrange
    // Act
    const base = getApiBaseUrl();

    // Assert
    expect(base).toBe("http://localhost:18943");
  });
});
