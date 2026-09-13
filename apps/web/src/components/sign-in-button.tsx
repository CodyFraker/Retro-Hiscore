"use client";

import { signIn } from "next-auth/react";
import { Button } from "@/components/ui/button";

type Props = {
  callbackUrl?: string;
};

export function SignInButton({ callbackUrl = "/" }: Props) {
  return (
    <Button
      type="button"
      onClick={() => {
        void (async () => {
          const result = await signIn("discord", { callbackUrl, redirect: false });
          if (result?.url) {
            window.location.href = result.url;
          }
        })();
      }}
    >
      Sign in with Discord
    </Button>
  );
}
