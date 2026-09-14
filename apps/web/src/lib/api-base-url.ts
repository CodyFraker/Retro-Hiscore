export function getApiBaseUrl() {
  if (typeof window === "undefined") {
    return (
      process.env.API_URL?.replace(/\/$/, "") ||
      process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ||
      "http://localhost:18943"
    );
  }

  return process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") || "";
}
