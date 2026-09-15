import { getServerSession } from "next-auth";
import { SiteHeaderBar } from "@/components/site-header-bar";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";

export async function SiteHeader() {
  const session = await getServerSession(authOptions);
  if (!session) {
    return null;
  }

  const displayName = session.user.discordUsername ?? session.user.name ?? "Signed in";
  const showAdminNav = session.isAdmin === true;

  let profileHref: string | null = null;
  try {
    const api = await getServerApiClient();
    const member = await api.getCurrentMember();
    if (member.raUsername) {
      profileHref = `/members/${encodeURIComponent(member.raUsername)}`;
    }
  } catch {
    profileHref = null;
  }

  return (
    <SiteHeaderBar
      displayName={displayName}
      avatarUrl={session.user.image}
      showAdminNav={showAdminNav}
      profileHref={profileHref}
    />
  );
}
