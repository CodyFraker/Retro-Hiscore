"use client";

import { Search } from "lucide-react";
import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { useSession } from "next-auth/react";
import type { SearchHitDto } from "@/generated/api-client";
import { getApiClient } from "@/lib/api";

export function GlobalSearch() {
  const { data: session } = useSession();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [hits, setHits] = useState<SearchHitDto[]>([]);
  const [pending, setPending] = useState(false);

  const runSearch = useCallback(async (value: string) => {
    const trimmed = value.trim();
    if (!trimmed) {
      setHits([]);
      return;
    }
    const token = session?.apiAccessToken;
    if (!token) {
      setHits([]);
      return;
    }
    setPending(true);
    try {
      const api = getApiClient({ accessToken: token });
      const response = await api.getSearch(trimmed, 20);
      setHits(response.hits);
    } catch {
      setHits([]);
    } finally {
      setPending(false);
    }
  }, [session?.apiAccessToken]);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setOpen(true);
      }
      if (event.key === "Escape") {
        setOpen(false);
      }
    }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, []);

  useEffect(() => {
    if (!open) {
      return;
    }
    const handle = window.setTimeout(() => {
      void runSearch(query);
    }, 200);
    return () => window.clearTimeout(handle);
  }, [open, query, runSearch]);

  return (
    <>
      <Button
        type="button"
        variant="outline"
        size="sm"
        className="hidden h-8 gap-2 text-muted-foreground md:inline-flex"
        onClick={() => setOpen(true)}
      >
        <Search className="size-4" />
        <span className="text-xs">Search</span>
        <kbd className="hidden rounded border border-border px-1 font-mono text-[10px] lg:inline">⌘K</kbd>
      </Button>
      <Button
        type="button"
        variant="ghost"
        size="icon"
        className="md:hidden"
        aria-label="Search"
        onClick={() => setOpen(true)}
      >
        <Search className="size-5" />
      </Button>

      {open ? (
        <div
          className="fixed inset-0 z-50 flex items-start justify-center bg-black/60 p-4 pt-[12vh]"
          role="dialog"
          aria-modal="true"
          aria-label="Search"
          onClick={() => setOpen(false)}
        >
          <div
            className="w-full max-w-lg rounded-lg border border-border bg-card shadow-xl"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="flex items-center gap-2 border-b border-border px-3">
              <Search className="size-4 shrink-0 text-muted-foreground" />
              <input
                type="search"
                autoFocus
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Games, members, leaderboards…"
                className="h-11 min-w-0 flex-1 bg-transparent text-sm outline-none"
              />
            </div>
            <ul className="max-h-80 overflow-y-auto p-2">
              {pending ? (
                <li className="px-2 py-3 text-sm text-muted-foreground">Searching…</li>
              ) : hits.length === 0 ? (
                <li className="px-2 py-3 text-sm text-muted-foreground">
                  {query.trim() ? "No results." : "Type to search."}
                </li>
              ) : (
                hits.map((hit) => (
                  <li key={`${hit.kind}-${hit.href}`}>
                    <Link
                      href={hit.href}
                      className="block rounded-md px-2 py-2 text-sm hover:bg-secondary/60"
                      onClick={() => setOpen(false)}
                    >
                      <span className="text-xs uppercase text-muted-foreground">{hit.kind}</span>
                      <p className="font-medium">{hit.title}</p>
                      {hit.subtitle ? (
                        <p className="text-xs text-muted-foreground">{hit.subtitle}</p>
                      ) : null}
                    </Link>
                  </li>
                ))
              )}
            </ul>
          </div>
        </div>
      ) : null}
    </>
  );
}
