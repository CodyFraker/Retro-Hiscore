import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

type Props = {
  children: ReactNode;
  className?: string;
  bleedOnMobile?: boolean;
};

export function ScrollRegion({ children, className, bleedOnMobile = false }: Props) {
  return (
    <div
      className={cn(
        "min-w-0 overflow-x-auto",
        bleedOnMobile && "-mx-4 px-4 sm:mx-0 sm:px-0",
        className,
      )}
    >
      {children}
    </div>
  );
}
