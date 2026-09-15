import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

type Props = {
  title: ReactNode;
  description?: ReactNode;
  className?: string;
  titleClassName?: string;
  actions?: ReactNode;
};

export function PageHero({ title, description, className, titleClassName, actions }: Props) {
  const heading = (
    <div className="min-w-0 space-y-2">
      <h1
        className={cn(
          "font-[family-name:var(--font-display)] text-2xl tracking-tight text-[var(--accent-retro)] sm:text-3xl md:text-4xl",
          titleClassName,
        )}
      >
        {title}
      </h1>
      {description ? <div className="max-w-2xl text-muted-foreground">{description}</div> : null}
    </div>
  );

  if (actions) {
    return (
      <section className={cn(className)}>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          {heading}
          {actions}
        </div>
      </section>
    );
  }

  return <section className={cn("space-y-2", className)}>{heading}</section>;
}
