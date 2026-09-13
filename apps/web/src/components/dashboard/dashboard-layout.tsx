import type { ReactNode } from "react";

type Props = {
  primary: ReactNode;
  sidebar?: ReactNode;
};

export function DashboardLayout({ primary, sidebar }: Props) {
  return (
    <div className="grid gap-8 lg:grid-cols-3 lg:items-start">
      <div className="space-y-8 lg:col-span-2">{primary}</div>
      {sidebar ? <aside className="space-y-6">{sidebar}</aside> : null}
    </div>
  );
}
