import { AddGameForm } from "@/components/add-game-form";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function AddGamePanel() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Track a game</CardTitle>
        <CardDescription>
          Add a RetroAchievements game to start syncing friend leaderboard scores.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <AddGameForm />
      </CardContent>
    </Card>
  );
}
