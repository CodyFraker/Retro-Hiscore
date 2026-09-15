import { getServerSession } from "next-auth";
import { SiteHeaderBar } from "@/components/site-header-bar";
import { authOptions } from "@/lib/auth-options";

export async function SiteHeader() {
  const session = await getServerSession(authOptions);
  if (!session) {
    return null;
  }

  const displayName = session.user.discordUsername ?? session.user.name ?? "Signed in";
  const showAdminNav = session.isAdmin === true;

  return (
    <SiteHeaderBar
      displayName={displayName}
      avatarUrl={session.user.image}
      showAdminNav={showAdminNav}
    />
  );
}
