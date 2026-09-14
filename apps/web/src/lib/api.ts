import { getServerSession } from "next-auth";
import { createApiClient } from "@/generated/api-client";
import { authOptions } from "@/lib/auth-options";
import { getApiBaseUrl } from "@/lib/api-base-url";

export { getApiBaseUrl };

function createAuthorizedFetch(accessToken?: string): typeof fetch {
  return (input, init) => {
    const headers = new Headers(init?.headers);
    if (accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
    }

    return fetch(input, {
      ...init,
      headers,
    });
  };
}

export function getApiClient(options?: { accessToken?: string }) {
  const fetchImpl = options?.accessToken
    ? createAuthorizedFetch(options.accessToken)
    : fetch;

  return createApiClient({ baseUrl: getApiBaseUrl(), fetch: fetchImpl });
}

export async function getServerApiClient() {
  const session = await getServerSession(authOptions);
  const accessToken = session?.apiAccessToken;

  if (!accessToken) {
    throw new Error("Not authenticated");
  }

  return getApiClient({ accessToken });
}
