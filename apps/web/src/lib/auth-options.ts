import type { NextAuthOptions } from "next-auth";
import DiscordProvider from "next-auth/providers/discord";
import { createApiAccessToken } from "@/lib/api-access-token";
import { createApiClient } from "@/generated/api-client";
import { getApiBaseUrl } from "@/lib/api-base-url";
import { isDiscordUserInvited } from "@/lib/sign-in-check";
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
      const discordId =
        profile?.id === undefined || profile?.id === null ? null : String(profile.id);
      return await isDiscordUserInvited(discordId ?? "");
    },
    async jwt({ token, account, profile, trigger, session }) {
      if (trigger === "update" && session) {
        if (session.needsOnboarding !== undefined) {
          token.needsOnboarding = session.needsOnboarding;
        }
        if (session.isAdmin !== undefined) {
          token.isAdmin = session.isAdmin;
        }
      }

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
          const baseUrl = getApiBaseUrl();
          if (baseUrl) {
            try {
              const api = createApiClient({
                baseUrl: getApiBaseUrl(),
                fetch: (input, init) => {
                  const headers = new Headers(init?.headers);
                  headers.set("Authorization", `Bearer ${token.apiAccessToken as string}`);
                  return fetch(input, { ...init, headers });
                },
              });
              const member = await api.getCurrentMember();
              token.needsOnboarding = member.needsOnboarding;
              token.isAdmin = member.isAdmin;
            } catch {
              token.needsOnboarding = true;
              token.isAdmin = false;
            }
          }
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
      session.needsOnboarding = token.needsOnboarding as boolean | undefined;
      session.isAdmin = token.isAdmin as boolean | undefined;
      return session;
    },
  },
};
