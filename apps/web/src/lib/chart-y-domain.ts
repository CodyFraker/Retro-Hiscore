export type ChartYDomainMode = "tight" | "fromZero";

export function tightYDomain(values: number[]): [number, number] {
  if (values.length === 0) {
    return [0, 1];
  }

  const min = Math.min(...values);
  const max = Math.max(...values);
  const span = max - min;

  if (span === 0) {
    const cushion = Math.max(Math.abs(min) * 0.05, 1);
    return [min - cushion, max + cushion];
  }

  const padding = Math.max(span * 0.08, 1);
  return [min - padding, max + padding];
}

export function fromZeroYDomain(values: number[]): [number, number] {
  if (values.length === 0) {
    return [0, 1];
  }

  const max = Math.max(...values);
  const min = Math.min(...values);
  const span = max - min;
  const padding = span === 0 ? Math.max(max * 0.08, 1) : Math.max(span * 0.08, 1);
  return [0, max + padding];
}

export function chartYDomain(values: number[], mode: ChartYDomainMode): [number, number] {
  return mode === "fromZero" ? fromZeroYDomain(values) : tightYDomain(values);
}
