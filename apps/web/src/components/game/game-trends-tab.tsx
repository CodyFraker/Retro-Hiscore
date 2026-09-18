"use client";

import { Fragment, type ReactNode } from "react";

type Props = {
  population: ReactNode;
  deltas: ReactNode;
  friendTrends: ReactNode;
};

const SECTIONS: { key: string; prop: keyof Props }[] = [
  { key: "population", prop: "population" },
  { key: "deltas", prop: "deltas" },
  { key: "friend-trends", prop: "friendTrends" },
];

export function GameTrendsTab({ population, deltas, friendTrends }: Props) {
  const panels: Props = { population, deltas, friendTrends };

  return (
    <div className="space-y-10">
      {SECTIONS.map(({ key, prop }) => (
        <Fragment key={key}>{panels[prop]}</Fragment>
      ))}
    </div>
  );
}
