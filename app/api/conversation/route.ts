import { NextResponse } from "next/server";

const TAVUS_API_URL = "https://tavusapi.com/v2/conversations";

export async function POST(request: Request) {
  const apiKey = process.env.TAVUS_API_KEY;
  const personaId = process.env.TAVUS_PERSONA_ID;

  if (!apiKey || !personaId) {
    return NextResponse.json(
      {
        error:
          "Missing Tavus configuration. Please set TAVUS_API_KEY and TAVUS_PERSONA_ID on the server.",
      },
      { status: 500 }
    );
  }

  let incomingBody: { conversationName?: string } | undefined;
  try {
    incomingBody = await request.json();
  } catch {
    // Ignore JSON parse errors – the endpoint can be called without a body.
  }

  const conversationName = incomingBody?.conversationName?.trim() || "Interview User";

  const tavusResponse = await fetch(TAVUS_API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "x-api-key": apiKey,
    },
    body: JSON.stringify({
      persona_id: personaId,
      conversation_name: conversationName,
    }),
  });

  const payload = await tavusResponse
    .json()
    .catch(() => ({ error: "Unable to parse Tavus response" }));

  if (!tavusResponse.ok) {
    return NextResponse.json(
      {
        error: "Unable to create Tavus conversation.",
        details: payload,
      },
      { status: tavusResponse.status }
    );
  }

  return NextResponse.json(payload);
}
