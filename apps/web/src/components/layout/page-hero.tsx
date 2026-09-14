import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

type Props = {
  title: ReactNode;
  description?: ReactNode;
  className?: string;
  titleClassName?: string;
};

export function PageHero({ title, description, className, titleClassName }: Props) {
  return (
    <section className={cn("space-y-2", className)}>
      <h1
        className={cn(
          "font-[family-name:var(--font-display)] text-2xl tracking-tight text-[var(--accent-retro)] sm:text-3xl md:text-4xl",
          titleClassName,
        )}
      >
        {title}
      </h1>
      {description ? <div className="max-w-2xl text-muted-foreground">{description}</div> : null}
    </section>
  );
}
