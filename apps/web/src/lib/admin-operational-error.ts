export const ADMIN_SCHEMA_OUT_OF_DATE_MESSAGE =
  "Database schema is out of date. Redeploy the API so migrations can run.";

export const ADMIN_GENERIC_DISPATCH_FAILURE_MESSAGE =
  "Discord dispatch failed. See server logs for details.";

export function summarizeAdminOperationalError(error: string | null | undefined): string {
  if (!error?.trim()) {
    return ADMIN_GENERIC_DISPATCH_FAILURE_MESSAGE;
  }

  const trimmed = error.trim();
  if (
    trimmed.includes("42703:") ||
    (trimmed.toLowerCase().includes("column") && trimmed.toLowerCase().includes("does not exist"))
  ) {
    return ADMIN_SCHEMA_OUT_OF_DATE_MESSAGE;
  }

  if (
    trimmed.length <= 120 &&
    !trimmed.includes("Npgsql") &&
    !trimmed.includes("PostgresException")
  ) {
    return trimmed;
  }

  return ADMIN_GENERIC_DISPATCH_FAILURE_MESSAGE;
}

export function truncateAdminError(error: string, max = 80): string {
  if (error.length <= max) {
    return error;
  }
  return `${error.slice(0, max)}…`;
}
