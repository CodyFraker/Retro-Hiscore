import "next-auth";

declare module "next-auth" {
  interface Profile {
    id?: string;
    username?: string;
  }

  interface Session {
    apiAccessToken?: string;
    needsOnboarding?: boolean;
    isAdmin?: boolean;
    user: {
      name?: string | null;
      email?: string | null;
      image?: string | null;
      discordId?: string;
      discordUsername?: string;
    };
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    apiAccessToken?: string;
    discordId?: string;
    discordUsername?: string;
    needsOnboarding?: boolean;
    isAdmin?: boolean;
  }
}
