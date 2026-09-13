import "next-auth";

declare module "next-auth" {
  interface Session {
    apiAccessToken?: string;
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
  }
}
