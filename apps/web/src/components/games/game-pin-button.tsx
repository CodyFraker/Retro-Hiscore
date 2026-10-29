"use client";

import { Pin, PinOff } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@/components/ui/button";
import { pinGameAction, unpinGameAction } from "@/lib/actions/member-pins";

type Props = {
  raGameId: number;
  pinned: boolean;
  variant?: "default" | "outline" | "ghost";
  size?: "default" | "sm" | "icon";
};

export function GamePinButton({ raGameId, pinned, variant = "outline", size = "sm" }: Props) {
  const router = useRouter();
  const [message, setMessage] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  return (
    <div className="flex flex-col items-end gap-1">
      <Button
        type="button"
        variant={variant}
        size={size}
        disabled={pending}
        onClick={() => {
          setMessage(null);
          startTransition(async () => {
            const result = pinned ? await unpinGameAction(raGameId) : await pinGameAction(raGameId);
            if (!result.ok) {
              setMessage(result.message);
              return;
            }
            router.refresh();
          });
        }}
      >
        {pinned ? <PinOff className="size-4" /> : <Pin className="size-4" />}
        {size !== "icon" ? (pinned ? "Unpin" : "Pin") : null}
      </Button>
      {message ? <p className="text-xs text-destructive">{message}</p> : null}
    </div>
  );
}
