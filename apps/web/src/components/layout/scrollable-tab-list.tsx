import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

type Props = {
  children: ReactNode;
  className?: string;
  listClassName?: string;
  ariaLabel?: string;
};

export function ScrollableTabList({ children, className, listClassName, ariaLabel }: Props) {
  return (
    <nav className={cn("border-b border-border", className)} aria-label={ariaLabel}>
      <ul
        className={cn(
          "-mx-4 mb-0 flex gap-4 overflow-x-auto scroll-smooth px-4 sm:gap-6",
          listClassName,
        )}
      >
        {children}
      </ul>
    </nav>
  );
}

export function ScrollableTabItem({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return <li className={cn("shrink-0", className)}>{children}</li>;
}

export function TabLinkLabel({ label, shortLabel }: { label: string; shortLabel?: string }) {
  if (!shortLabel) {
    return <>{label}</>;
  }

  return (
    <>
      <span className="sm:hidden">{shortLabel}</span>
      <span className="hidden sm:inline">{label}</span>
    </>
  );
}
