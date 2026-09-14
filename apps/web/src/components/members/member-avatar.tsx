import { User } from "lucide-react";
import Image from "next/image";

type Props = {
  avatarUrl?: string | null;
  displayName: string;
  size?: number;
  className?: string;
};

export function MemberAvatar({ avatarUrl, displayName, size = 28, className = "" }: Props) {
  if (avatarUrl) {
    return (
      <Image
        src={avatarUrl}
        alt={displayName}
        width={size}
        height={size}
        className={`shrink-0 rounded-full ${className}`}
      />
    );
  }

  return (
    <span
      className={`inline-flex shrink-0 items-center justify-center rounded-full bg-secondary text-muted-foreground ${className}`}
      style={{ width: size, height: size }}
      aria-hidden
    >
      <User className="size-[55%]" />
    </span>
  );
}
