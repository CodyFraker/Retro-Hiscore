import type { ReactNode } from "react";

type Props = {
  header?: ReactNode;
  primary: ReactNode;
  sidebar?: ReactNode;
};

export function DashboardLayout({ header, primary, sidebar }: Props) {
  if (!sidebar) {
    return (
      <div className="space-y-8">
        {header}
        {primary}
      </div>
    );
  }

  return (
    <div className="grid gap-8 lg:grid-cols-3 lg:items-start">
      <div className="space-y-8 lg:col-span-2">
        {header}
        {primary}
      </div>
      <aside className="space-y-6">{sidebar}</aside>
    </div>
  );
}
