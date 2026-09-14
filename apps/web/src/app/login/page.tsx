import { AlertCircle } from "lucide-react";
import { getServerSession } from "next-auth";
import { redirect } from "next/navigation";
import { SignInButton } from "@/components/sign-in-button";
import { PageHero } from "@/components/layout/page-hero";
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
      ? "Your Discord account has not been invited to this site yet."
      : params.error
        ? "Sign-in failed. Try again or contact the site owner."
        : null;

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6 py-16">
      <PageHero
        title="Sign in"
        titleClassName="text-center sm:text-left"
        className="text-center sm:text-left"
        description="Use Discord to sign in. Only invited Discord accounts can access this site."
      />

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
