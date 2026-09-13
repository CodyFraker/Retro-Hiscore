"use client";

import { useSession } from "next-auth/react";
import { useMemo } from "react";
import { getApiClient } from "@/lib/api";

export function useApiClient() {
  const { data: session } = useSession();
  const accessToken = session?.apiAccessToken;

  return useMemo(() => getApiClient({ accessToken }), [accessToken]);
}
