"use client";

import { RefreshCw } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { useApiClient } from "@/lib/use-api-client";

type Props = {
  disabledReason?: string | null;
  variant?: "button" | "menu";
};

export function RefreshButton({ disabledReason, variant = "button" }: Props) {
  const api = useApiClient();
  const router = useRouter();
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const runSync = () => {
    startTransition(async () => {
      try {
        const result = await api.triggerSync();
        if (!result.ok) {
          setMessage(result.body.message);
          return;
        }
        setMessage("Sync queued");
        router.refresh();
      } catch (error) {
        setMessage(error instanceof Error ? error.message : "Sync failed");
      }
    });
  };

  if (variant === "menu") {
    return (
      <DropdownMenuItem disabled={pending} onClick={runSync}>
        <RefreshCw className={pending ? "animate-spin" : undefined} />
        {pending ? "Refreshing…" : "Refresh scores"}
      </DropdownMenuItem>
    );
  }

  return (
    <div className="flex flex-col items-end gap-1">
      <Button type="button" disabled={pending} onClick={runSync}>
        <RefreshCw className={pending ? "animate-spin" : undefined} />
        {pending ? "Refreshing…" : "Refresh scores"}
      </Button>
      {(message || disabledReason) && (
        <p className="text-xs text-muted-foreground">{message ?? disabledReason}</p>
      )}
    </div>
  );
}
