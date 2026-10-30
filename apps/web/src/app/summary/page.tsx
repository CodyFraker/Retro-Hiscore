import { redirect } from "next/navigation";

export const dynamic = "force-dynamic";

export default function SummaryIndexPage() {
  const now = new Date();
  redirect(`/summary/${now.getUTCFullYear()}/${now.getUTCMonth() + 1}`);
}
