import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";
import { DEFAULT_THEME, resolveTheme, type ThemeId } from "@/lib/theme";

export type LayoutThemeState = {
  initialTheme: ThemeId;
  syncToAccount: boolean;
  serverTheme: ThemeId | null;
};

export async function getLayoutThemeState(): Promise<LayoutThemeState> {
  const session = await getServerSession(authOptions);
  if (!session) {
    return {
      initialTheme: DEFAULT_THEME,
      syncToAccount: false,
      serverTheme: null,
    };
  }

  try {
    const api = await getServerApiClient();
    const member = await api.getCurrentMember();
    const theme = resolveTheme(member.uiTheme);
    return {
      initialTheme: theme,
      syncToAccount: true,
      serverTheme: theme,
    };
  } catch {
    return {
      initialTheme: DEFAULT_THEME,
      syncToAccount: false,
      serverTheme: null,
    };
  }
}
