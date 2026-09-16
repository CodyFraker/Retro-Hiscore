import Link from "next/link";
import { rivalryPath } from "@/lib/rivalry-path";

type Props = {
  currentRaUsername: string;
  otherRaUsername: string;
  otherDisplayName: string;
};

export function MemberCompareLink({ currentRaUsername, otherRaUsername, otherDisplayName }: Props) {
  if (currentRaUsername.toLowerCase() === otherRaUsername.toLowerCase()) {
    return null;
  }

  return (
    <p className="text-sm">
      <Link
        href={rivalryPath(currentRaUsername, otherRaUsername)}
        className="font-medium text-[var(--accent-retro)] hover:underline"
      >
        Compare with {otherDisplayName} →
      </Link>
    </p>
  );
}
