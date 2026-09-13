"use client";

import { signOut } from "next-auth/react";
import { LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";

export function SignOutButton() {
  return (
    <Button
      type="button"
      variant="outline"
      size="sm"
      onClick={() => {
        void signOut({ callbackUrl: "/login" });
      }}
    >
      <LogOut />
      Sign out
    </Button>
  );
}
