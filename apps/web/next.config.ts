import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
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
