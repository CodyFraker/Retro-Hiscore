import {
  summarizeAdminOperationalError,
  truncateAdminError,
} from "@/lib/admin-operational-error";

type Props = {
  error: string | null | undefined;
};

export function AdminRunErrorDetails({ error }: Props) {
  if (!error) {
    return null;
  }

  const summary = summarizeAdminOperationalError(error);
  const showDetails = summary !== error;

  if (!showDetails) {
    return <p className="text-sm text-destructive">{summary}</p>;
  }

  return (
    <details className="w-full text-sm">
      <summary className="cursor-pointer text-destructive">{summary}</summary>
      <p className="mt-1 whitespace-pre-wrap break-words text-xs text-muted-foreground">
        {error}
      </p>
    </details>
  );
}

export function AdminRunErrorSummaryLine({ error }: Props) {
  if (!error) {
    return null;
  }

  const summary = summarizeAdminOperationalError(error);

  return (
    <details className="w-full text-xs">
      <summary className="cursor-pointer text-destructive">{truncateAdminError(summary)}</summary>
      {summary !== error ? (
        <p className="mt-1 whitespace-pre-wrap break-words text-muted-foreground">{error}</p>
      ) : null}
    </details>
  );
}
