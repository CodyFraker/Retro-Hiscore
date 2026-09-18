import { notFound } from "next/navigation";
import { AdminThemeLabPanel } from "@/components/admin/admin-theme-lab-panel";
import { isThemeLabEnabled } from "@/lib/theme-lab-env";

export const dynamic = "force-dynamic";

export default function AdminThemeLabPage() {
  if (!isThemeLabEnabled()) {
    notFound();
  }

  return <AdminThemeLabPanel />;
}
