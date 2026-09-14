import Image from "next/image";

type ConsoleNameProps = {
  name?: string | null;
  iconUrl?: string | null;
  fallback?: string;
};

export function ConsoleName({ name, iconUrl, fallback }: ConsoleNameProps) {
  const label = name ?? fallback;
  if (!label) {
    return null;
  }

  return (
    <span className="inline-flex items-center gap-1.5 text-sm text-muted-foreground">
      {iconUrl ? (
        <Image src={iconUrl} alt="" width={16} height={16} className="size-4 shrink-0" />
      ) : null}
      <span>{label}</span>
    </span>
  );
}
