import { Settings } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";

type Props = {
  onNavigate?: () => void;
};

export function SettingsLinkButton({ onNavigate }: Props) {
  return (
    <Button variant="outline" size="sm" render={<Link href="/settings" onClick={onNavigate} />}>
      <Settings />
      Settings
    </Button>
  );
}
