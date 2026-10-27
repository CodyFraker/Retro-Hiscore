import { createApiClient } from "@/generated/api-client";
import { getApiBaseUrl } from "@/lib/api-base-url";
import { redirectToSignInAfterInvalidSession } from "@/lib/invalid-session";
import { getServerApiAccessToken } from "@/lib/server-api-access-token";

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
  const accessToken = await getServerApiAccessToken();

  if (!accessToken) {
    redirectToSignInAfterInvalidSession();
  }

  return getApiClient({ accessToken });
}
