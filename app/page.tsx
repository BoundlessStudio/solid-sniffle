"use client";

import { useState } from "react";

export default function Home() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [conversationLink, setConversationLink] = useState<string | null>(null);

  const startConversation = async () => {
    setIsLoading(true);
    setError(null);
    setConversationLink(null);

    try {
      const response = await fetch("/api/conversation", {
        method: "POST",
      });

      if (!response.ok) {
        const details = await response.json().catch(() => null);
        throw new Error(details?.error || "Unable to start conversation");
      }

      const data = await response.json();
      setConversationLink(data?.conversation_url);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unknown error";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-950 via-slate-900 to-slate-950 text-white">
      <div className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-8 px-6 text-center">
        <header className="space-y-4">
          <p className="text-sm uppercase tracking-[0.3em] text-slate-400">Public demo</p>
          <h1 className="text-4xl font-semibold leading-tight sm:text-5xl">
            Start a conversation with your Tavus persona
          </h1>
          <p className="text-lg text-slate-300">
            Press the button below to spin up a fresh meeting link powered by the Tavus Conversations API.
          </p>
        </header>

        <button
          onClick={startConversation}
          disabled={isLoading}
          className="w-full max-w-md rounded-full bg-white/90 px-8 py-5 text-lg font-medium text-slate-900 transition hover:bg-white focus:outline-none focus-visible:ring-4 focus-visible:ring-slate-200 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isLoading ? "Creating conversation..." : "Start conversation"}
        </button>

        {error && (
          <p className="w-full max-w-xl rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-100">
            {error}
          </p>
        )}

        {conversationLink && (
          <a
            href={conversationLink}
            target="_blank"
            rel="noreferrer"
            className="w-full max-w-md rounded-2xl border border-emerald-400/40 bg-emerald-500/10 px-6 py-4 text-base font-medium text-emerald-100 underline"
          >
            Join your conversation ↗
          </a>
        )}
      </div>
    </div>
  );
}
