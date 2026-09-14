import type { NextAuthOptions } from "next-auth";
import DiscordProvider from "next-auth/providers/discord";
import { createApiAccessToken } from "@/lib/api-access-token";
import { isDiscordUserAllowlisted } from "@/lib/allowed-discord-users";
import { syncMemberProfileOnLogin } from "@/lib/sync-member-profile";

export const authOptions: NextAuthOptions = {
  secret: process.env.AUTH_SECRET ?? process.env.NEXTAUTH_SECRET,
  providers: [
    DiscordProvider({
      clientId: process.env.AUTH_DISCORD_ID ?? "",
      clientSecret: process.env.AUTH_DISCORD_SECRET ?? "",
    }),
  ],
  pages: {
    signIn: "/login",
    error: "/login",
  },
  callbacks: {
    async signIn({ profile }) {
      return isDiscordUserAllowlisted(
        profile?.id === undefined || profile?.id === null ? null : String(profile.id),
      );
    },
    async jwt({ token, account, profile }) {
      if (account && profile?.id !== undefined && profile?.id !== null) {
        const discordId = String(profile.id);
        const username =
          typeof profile.username === "string"
            ? profile.username
            : profile.name ?? "discord-user";
        const avatarUrl =
          typeof profile.image === "string" ? profile.image : token.picture?.toString() ?? null;
        token.apiAccessToken = await createApiAccessToken(discordId, username);
        token.discordId = discordId;
        token.discordUsername = username;
        token.picture = avatarUrl;
        if (token.apiAccessToken) {
          await syncMemberProfileOnLogin(token.apiAccessToken, avatarUrl);
        }
      }

      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.discordId = token.discordId as string | undefined;
        session.user.discordUsername = token.discordUsername as string | undefined;
      }

      session.apiAccessToken = token.apiAccessToken as string | undefined;
      return session;
    },
  },
};
