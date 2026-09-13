import { AlertCircle } from "lucide-react";
import { getServerSession } from "next-auth";
import { redirect } from "next/navigation";
import { SignInButton } from "@/components/sign-in-button";
import { authOptions } from "@/lib/auth-options";

type Props = {
  searchParams: Promise<{ callbackUrl?: string; error?: string }>;
};

export default async function LoginPage({ searchParams }: Props) {
  const params = await searchParams;
  const session = await getServerSession(authOptions);

  if (session) {
    redirect(params.callbackUrl ?? "/");
  }

  const errorMessage =
    params.error === "AccessDenied"
      ? "Your Discord account is not on the allowlist for this site."
      : params.error
        ? "Sign-in failed. Try again or contact the site owner."
        : null;

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6 py-16">
      <div className="space-y-2 text-center">
        <h1 className="font-[family-name:var(--font-display)] text-3xl tracking-tight text-[var(--accent-retro)]">
          Sign in
        </h1>
        <p className="text-sm text-muted-foreground">
          Use Discord to access Retro Hiscore. Only allowlisted friends can sign in.
        </p>
      </div>

      {errorMessage && (
        <p className="flex items-center gap-2 rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          <AlertCircle className="size-4 shrink-0" />
          {errorMessage}
        </p>
      )}

      <div className="flex justify-center">
        <SignInButton callbackUrl={params.callbackUrl ?? "/"} />
      </div>
    </div>
  );
}
