import { sandwichResponseSchema, validateModelResponse, validateRequestBody } from "./schemas";
import { SYSTEM_PROMPT } from "./systemPrompt";

interface Env {
  OPENAI_API_KEY: string;
  OPENAI_MODEL?: string;
  ALLOWED_ORIGINS?: string;
}

interface OpenAIResponse {
  output?: Array<{ content?: Array<{ type?: string; text?: string }> }>;
}

const DEFAULT_ORIGINS = [
  "http://localhost:5000",
  "http://localhost:8080",
  "http://127.0.0.1:5000",
];

function json(body: unknown, status: number, headers: Headers): Response {
  headers.set("Content-Type", "application/json; charset=utf-8");
  return new Response(JSON.stringify(body), { status, headers });
}

function corsHeaders(request: Request, env: Env): { headers: Headers; allowed: boolean } {
  const headers = new Headers({ Vary: "Origin" });
  const origin = request.headers.get("Origin");
  const allowedOrigins = (env.ALLOWED_ORIGINS ?? DEFAULT_ORIGINS.join(","))
    .split(",")
    .map((value) => value.trim())
    .filter(Boolean);
  const allowed = !origin || allowedOrigins.includes(origin);

  if (origin && allowed) headers.set("Access-Control-Allow-Origin", origin);
  headers.set("Access-Control-Allow-Headers", "Content-Type");
  headers.set("Access-Control-Allow-Methods", "POST, OPTIONS");
  return { headers, allowed };
}

function outputText(response: OpenAIResponse): string | null {
  for (const item of response.output ?? []) {
    for (const content of item.content ?? []) {
      if (content.type === "output_text" && typeof content.text === "string") return content.text;
    }
  }
  return null;
}

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const cors = corsHeaders(request, env);
    if (request.method === "OPTIONS") {
      return new Response(null, { status: cors.allowed ? 204 : 403, headers: cors.headers });
    }
    if (request.method !== "POST") return json({ error: "Method not allowed." }, 405, cors.headers);
    if (!cors.allowed) return json({ error: "Origin not allowed." }, 403, cors.headers);

    let body: unknown;
    try {
      body = await request.json();
    } catch {
      return json({ error: "Invalid request." }, 400, cors.headers);
    }
    const input = validateRequestBody(body);
    if (!input) return json({ error: "Invalid request." }, 400, cors.headers);
    if (!env.OPENAI_API_KEY) return json({ error: "Server is not configured." }, 500, cors.headers);

    try {
      const openAIResponse = await fetch("https://api.openai.com/v1/responses", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${env.OPENAI_API_KEY}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          model: env.OPENAI_MODEL ?? "gpt-5.6-sol",
          instructions: SYSTEM_PROMPT,
          input: JSON.stringify(input),
          max_output_tokens: 500,
          text: {
            format: {
              type: "json_schema",
              name: "sandwich_actions",
              strict: true,
              schema: sandwichResponseSchema,
            },
          },
        }),
      });

      if (!openAIResponse.ok) {
        console.error("OpenAI request failed", { status: openAIResponse.status });
        return json({ error: "Command interpretation failed." }, 502, cors.headers);
      }

      const response = (await openAIResponse.json()) as OpenAIResponse;
      const text = outputText(response);
      const parsed: unknown = text ? JSON.parse(text) : null;
      if (!validateModelResponse(parsed)) throw new Error("Invalid model output");
      return json(parsed, 200, cors.headers);
    } catch (error) {
      console.error("Command interpretation failed", {
        errorType: error instanceof Error ? error.name : "unknown",
      });
      return json({ error: "Command interpretation failed." }, 502, cors.headers);
    }
  },
} satisfies ExportedHandler<Env>;
