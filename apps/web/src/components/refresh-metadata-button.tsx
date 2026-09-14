"use client";

import { Image } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { triggerMetadataSyncAction } from "@/lib/actions/sync";

type Props = {
  variant?: "button" | "menu";
};

export function RefreshMetadataButton({ variant = "button" }: Props) {
  const router = useRouter();
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const runSync = () => {
    startTransition(async () => {
      const result = await triggerMetadataSyncAction();
      if (!result.ok) {
        setMessage(result.message);
        return;
      }
      setMessage(result.message);
      router.refresh();
    });
  };

  if (variant === "menu") {
    return (
      <DropdownMenuItem disabled={pending} onClick={runSync}>
        <Image />
        {pending ? "Updating art…" : "Refresh game art"}
      </DropdownMenuItem>
    );
  }

  return (
    <div className="flex flex-col items-end gap-1">
      <Button type="button" variant="secondary" disabled={pending} onClick={runSync}>
        <Image />
        {pending ? "Updating art…" : "Refresh game art"}
      </Button>
      {message && <p className="text-xs text-muted-foreground">{message}</p>}
    </div>
  );
}
