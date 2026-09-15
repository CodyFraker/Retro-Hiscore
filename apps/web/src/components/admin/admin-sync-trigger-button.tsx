"use client";

import type { LucideIcon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useTransition } from "react";
import { Button } from "@/components/ui/button";
import type { SyncActionResult } from "@/lib/actions/sync";

type Props = {
  label: string;
  pendingLabel: string;
  icon?: LucideIcon;
  variant?: "default" | "secondary" | "outline";
  disabled?: boolean;
  onTrigger: () => Promise<SyncActionResult>;
  onMessage?: (message: string | null) => void;
};

export function AdminSyncTriggerButton({
  label,
  pendingLabel,
  icon: Icon,
  variant = "default",
  disabled,
  onTrigger,
  onMessage,
}: Props) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();

  return (
    <Button
      type="button"
      variant={variant}
      disabled={pending || disabled}
      onClick={() => {
        startTransition(async () => {
          const result = await onTrigger();
          onMessage?.(result.message);
          if (result.ok) {
            router.refresh();
          }
        });
      }}
    >
      {Icon ? (
        <Icon className={pending ? "animate-spin" : undefined} aria-hidden />
      ) : null}
      {pending ? pendingLabel : label}
    </Button>
  );
}
