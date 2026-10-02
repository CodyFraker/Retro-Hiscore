import { getServerSession } from "next-auth";
import { createApiClient } from "@/generated/api-client";
import { authOptions } from "@/lib/auth-options";
import { getApiBaseUrl } from "@/lib/api-base-url";
import { redirectToSignInAfterInvalidSession } from "@/lib/invalid-session";

export { getApiBaseUrl };

function createAuthorizedFetch(accessToken?: string): typeof fetch {
  return async (input, init) => {
    const headers = new Headers(init?.headers);
    if (accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
    }

    const response = await fetch(input, {
      ...init,
      headers,
    });

    if (response.status === 401 && accessToken) {
      redirectToSignInAfterInvalidSession();
    }

    return response;
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
    redirectToSignInAfterInvalidSession();
  }

  return getApiClient({ accessToken });
}
