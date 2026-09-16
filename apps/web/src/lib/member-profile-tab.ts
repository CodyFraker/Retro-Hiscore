export const MEMBER_PROFILE_TABS = ["overview", "leaderboards", "achievements"] as const;

export type MemberProfileTabId = (typeof MEMBER_PROFILE_TABS)[number];

export function resolveMemberProfileTab(raw: string | undefined): MemberProfileTabId {
  if (raw && (MEMBER_PROFILE_TABS as readonly string[]).includes(raw)) {
    return raw as MemberProfileTabId;
  }
  return "overview";
}
