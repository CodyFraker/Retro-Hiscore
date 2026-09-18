import { DEFAULT_THEME, STORAGE_KEY, THEME_IDS } from "@/lib/theme";

type Props = {
  enabled: boolean;
};

export function ThemeInitScript({ enabled }: Props) {
  if (!enabled) {
    return null;
  }

  const allowed = JSON.stringify(THEME_IDS);
  const script = `(function(){try{var k=${JSON.stringify(STORAGE_KEY)};var d=${JSON.stringify(DEFAULT_THEME)};var allowed=${allowed};var v=localStorage.getItem(k);var t=allowed.indexOf(v)>=0?v:d;document.documentElement.dataset.theme=t;}catch(e){document.documentElement.dataset.theme=${JSON.stringify(DEFAULT_THEME)};}})();`;

  return <script dangerouslySetInnerHTML={{ __html: script }} />;
}
