const DEFAULT_SERVER_API_URL = "http://localhost:18943";

export function getApiBaseUrl() {
  if (typeof window === "undefined") {
    return (
      process.env.API_URL?.replace(/\/$/, "") ||
      DEFAULT_SERVER_API_URL
    );
  }

  return "";
}
