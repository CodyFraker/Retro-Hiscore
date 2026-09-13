import { Card, CardContent, CardHeader } from "@/components/ui/card";

export default function HomeLoading() {
  return (
    <div className="space-y-8">
      <div className="space-y-2">
        <div className="h-10 w-56 animate-pulse rounded bg-muted" />
        <div className="h-4 w-full max-w-2xl animate-pulse rounded bg-muted" />
      </div>

      <div className="grid gap-8 lg:grid-cols-3 lg:items-start">
        <div className="space-y-8 lg:col-span-2">
          <Card>
            <CardHeader>
              <div className="h-5 w-32 animate-pulse rounded bg-muted" />
              <div className="h-4 w-64 animate-pulse rounded bg-muted" />
            </CardHeader>
            <CardContent>
              <div className="h-8 w-full animate-pulse rounded bg-muted" />
            </CardContent>
          </Card>

          <div className="space-y-3">
            <div className="h-6 w-40 animate-pulse rounded bg-muted" />
            <Card>
              <CardContent className="space-y-4 py-6">
                {Array.from({ length: 4 }).map((_, index) => (
                  <div key={index} className="flex items-center gap-4">
                    <div className="size-16 animate-pulse rounded bg-muted" />
                    <div className="flex-1 space-y-2">
                      <div className="h-5 w-48 animate-pulse rounded bg-muted" />
                      <div className="h-4 w-32 animate-pulse rounded bg-muted" />
                    </div>
                  </div>
                ))}
              </CardContent>
            </Card>
          </div>
        </div>

        <aside className="space-y-6">
          {Array.from({ length: 2 }).map((_, index) => (
            <Card key={index}>
              <CardHeader>
                <div className="h-5 w-40 animate-pulse rounded bg-muted" />
              </CardHeader>
              <CardContent className="space-y-3">
                {Array.from({ length: 3 }).map((__, rowIndex) => (
                  <div key={rowIndex} className="h-4 w-full animate-pulse rounded bg-muted" />
                ))}
              </CardContent>
            </Card>
          ))}
        </aside>
      </div>
    </div>
  );
}
