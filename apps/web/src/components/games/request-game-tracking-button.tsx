"use client";

import { useRouter } from "next/navigation";
import { useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { postGameTrackRequestAction } from "@/lib/actions/track-requests";

type Props = {
  raGameId: number;
  title: string;
  disabled?: boolean;
};

export function RequestGameTrackingButton({ raGameId, title, disabled }: Props) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();

  return (
    <Button
      type="button"
      size="sm"
      variant="outline"
      disabled={disabled || pending}
      onClick={() => {
        startTransition(async () => {
          const result = await postGameTrackRequestAction(raGameId);
          if (!result.ok) {
            toast.error(result.error);
            return;
          }

          const { response } = result;
          if (response.code === "alreadyTracked") {
            toast.message(`${response.title} is already tracked.`, {
              action: {
                label: "View game",
                onClick: () => router.push(`/games/${response.raGameId}`),
              },
            });
            return;
          }

          if (response.code === "quotaExceeded") {
            toast.error(response.message ?? "Request limit reached.");
            return;
          }

          if (response.code === "created") {
            toast.success(`Requested ${title}.`);
          } else if (response.code === "alreadyPending") {
            toast.message(`${title} is already on the track queue.`);
          }

          router.refresh();
        });
      }}
    >
      {pending ? "Requesting…" : "Request tracking"}
    </Button>
  );
}
