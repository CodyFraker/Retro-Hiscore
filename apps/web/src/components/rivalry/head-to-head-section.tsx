import type { MemberDto } from "@/generated/api-client";
import { RivalryPicker } from "@/components/rivalry/rivalry-picker";

type Props = {
  members: MemberDto[];
};

export function HeadToHeadSection({ members }: Props) {
  const selectable = members.filter((member) => member.raUsername);

  return (
    <section id="head-to-head" className="space-y-3">
      <h2 className="steam-section-heading">Head-to-head</h2>
      {members.length < 2 ? (
        <p className="text-sm text-muted-foreground">
          Compare scores and board leads side by side once your group has two members with linked
          RetroAchievements accounts.
        </p>
      ) : selectable.length < 2 ? (
        <p className="text-sm text-muted-foreground">
          Head-to-head needs at least two members with linked RetroAchievements usernames.
        </p>
      ) : (
        <RivalryPicker members={members} />
      )}
    </section>
  );
}
