export function formatGameOfTheWeekPhase(phase: number): string {
  switch (phase) {
    case 0:
      return "Scheduled";
    case 1:
      return "Voting open";
    case 2:
      return "Closed";
    default:
      return `Phase ${phase}`;
  }
}

export function formatGameOfTheWeekTrackingStatus(status: number): string {
  switch (status) {
    case 0:
      return "Not applicable";
    case 1:
      return "Pending (importing winner)";
    case 2:
      return "Completed";
    default:
      return `Status ${status}`;
  }
}
