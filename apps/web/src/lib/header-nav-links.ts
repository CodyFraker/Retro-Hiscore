export type HeaderNavLink = {
  href: string;
  label: string;
  shortLabel?: string;
  match: (pathname: string) => boolean;
};

export const MAIN_HEADER_NAV_LINKS: HeaderNavLink[] = [
  { href: "/", label: "Home", match: (path) => path === "/" },
  { href: "/games", label: "Games", match: (path) => path === "/games" || path.startsWith("/games/") },
  {
    href: "/game-of-the-week",
    label: "Game of the week",
    shortLabel: "GOTW",
    match: (path) => path === "/game-of-the-week",
  },
  {
    href: "/achievements",
    label: "Achievements",
    match: (path) => path === "/achievements" || path.startsWith("/achievements/"),
  },
];

export const ACCOUNT_HEADER_NAV_LINKS: HeaderNavLink[] = [
  { href: "/members", label: "Members", match: (path) => path === "/members" || path.startsWith("/members/") },
  {
    href: "/rivalry",
    label: "Rivalry",
    match: (path) => path === "/rivalry" || path.startsWith("/rivalry/"),
  },
];

export const ADMIN_HEADER_NAV_LINK: HeaderNavLink = {
  href: "/admin/members",
  label: "Admin",
  match: (path) => path === "/admin" || path.startsWith("/admin/"),
};

export function accountHeaderNavLinks(showAdminNav?: boolean): HeaderNavLink[] {
  return showAdminNav
    ? [...ACCOUNT_HEADER_NAV_LINKS, ADMIN_HEADER_NAV_LINK]
    : ACCOUNT_HEADER_NAV_LINKS;
}
