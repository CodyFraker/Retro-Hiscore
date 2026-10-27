import type { NextConfig } from "next";
import { buildSecurityHeaders } from "./src/lib/security-headers";

const nextConfig: NextConfig = {
  output: "standalone",
  headers: async () => buildSecurityHeaders(),
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "media.retroachievements.org",
        pathname: "/Images/**",
      },
      {
        protocol: "https",
        hostname: "media.retroachievements.org",
        pathname: "/Badge/**",
      },
      {
        protocol: "https",
        hostname: "media.retroachievements.org",
        pathname: "/UserPic/**",
      },
      {
        protocol: "https",
        hostname: "retroachievements.org",
        pathname: "/UserPic/**",
      },
      {
        protocol: "https",
        hostname: "cdn.discordapp.com",
        pathname: "/**",
      },
    ],
  },
};

export default nextConfig;
