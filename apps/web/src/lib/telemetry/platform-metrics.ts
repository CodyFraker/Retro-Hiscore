import { metrics } from "@opentelemetry/api";
import { pathnameToRouteTemplate } from "@/lib/telemetry/route-template";

const METER_NAME = "RetroHiscore.Platform.Web";

let pageViewCounter:
  | ReturnType<ReturnType<typeof metrics.getMeter>["createCounter"]>
  | undefined;
let authSignInSuccessCounter:
  | ReturnType<ReturnType<typeof metrics.getMeter>["createCounter"]>
  | undefined;
let authSignInRejectedCounter:
  | ReturnType<ReturnType<typeof metrics.getMeter>["createCounter"]>
  | undefined;

function meter() {
  return metrics.getMeter(METER_NAME);
}

function ensureCounters() {
  if (!pageViewCounter) {
    pageViewCounter = meter().createCounter("page.view", {
      description: "Page views by normalized route",
    });
  }

  if (!authSignInSuccessCounter) {
    authSignInSuccessCounter = meter().createCounter("auth.sign_in.success", {
      description: "Successful member sign-ins",
    });
  }

  if (!authSignInRejectedCounter) {
    authSignInRejectedCounter = meter().createCounter("auth.sign_in.rejected_not_invited", {
      description: "Sign-in attempts rejected because the Discord user is not invited",
    });
  }
}

export function recordPageView(pathname: string, surface: "server" | "client") {
  ensureCounters();
  const route = pathnameToRouteTemplate(pathname);
  pageViewCounter!.add(1, { route, surface });
}

export function recordAuthSignInSuccess() {
  ensureCounters();
  authSignInSuccessCounter!.add(1);
}

export function recordAuthSignInRejectedNotInvited() {
  ensureCounters();
  authSignInRejectedCounter!.add(1);
}
