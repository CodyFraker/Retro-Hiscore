"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import type { MemberDto } from "@/generated/api-client";

type Props = {
  members: MemberDto[];
};

export function RivalryPicker({ members }: Props) {
  const router = useRouter();
  const selectable = members.filter((member) => member.raUsername);
  const [usernameA, setUsernameA] = useState(selectable[0]?.raUsername ?? "");
  const [usernameB, setUsernameB] = useState(selectable[1]?.raUsername ?? "");

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!usernameA || !usernameB || usernameA === usernameB) {
      return;
    }

    router.push(
      `/rivalry/${encodeURIComponent(usernameA)}/${encodeURIComponent(usernameB)}`,
    );
  }

  if (selectable.length < 2) {
    return null;
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
      <label className="w-full space-y-1 text-sm sm:w-auto">
        <span className="text-muted-foreground">Player A</span>
        <select
          value={usernameA}
          onChange={(event) => setUsernameA(event.target.value)}
          className="block w-full rounded-md bg-input px-3 py-2 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:w-auto"
        >
          {selectable.map((member) => (
            <option key={member.id} value={member.raUsername ?? ""}>
              {member.displayName}
            </option>
          ))}
        </select>
      </label>
      <label className="w-full space-y-1 text-sm sm:w-auto">
        <span className="text-muted-foreground">Player B</span>
        <select
          value={usernameB}
          onChange={(event) => setUsernameB(event.target.value)}
          className="block w-full rounded-md bg-input px-3 py-2 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:w-auto"
        >
          {selectable.map((member) => (
            <option key={member.id} value={member.raUsername ?? ""}>
              {member.displayName}
            </option>
          ))}
        </select>
      </label>
      <Button type="submit" className="w-full sm:w-auto" disabled={!usernameA || !usernameB || usernameA === usernameB}>
        Compare
      </Button>
    </form>
  );
}
