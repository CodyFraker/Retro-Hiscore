import { Card, CardContent, CardHeader } from "@/components/ui/card";

export function DashboardPageSkeleton() {
  return (
    <div className="space-y-8">
      <div className="space-y-2">
        <div className="h-10 w-40 animate-pulse rounded bg-muted" />
        <div className="h-4 w-full max-w-2xl animate-pulse rounded bg-muted" />
      </div>

      <div className="space-y-8">
        {Array.from({ length: 3 }).map((_, index) => (
          <Card key={index}>
            <CardHeader>
              <div className="h-5 w-48 animate-pulse rounded bg-muted" />
            </CardHeader>
            <CardContent className="space-y-3">
              {Array.from({ length: 4 }).map((__, rowIndex) => (
                <div key={rowIndex} className="h-4 w-full animate-pulse rounded bg-muted" />
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
