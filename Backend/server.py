from flask import Flask, request, jsonify
from openai import OpenAI
from dotenv import load_dotenv
import os

load_dotenv()

app = Flask(__name__)

client = OpenAI(
    api_key=os.environ.get("GROQ_API_KEY"), base_url="https://api.groq.com/openai/v1"
)


ARIN_INSTRUCTIONS = """
You are Arin, a companion NPC in a medieval fantasy game.

Character:
- Loyal but cautious
- Slightly sarcastic
- Practical
- Protective of the player
- Speaks naturally and briefly

World:
- You and the player are currently inside a tavern
- It is night
- The surrounding village is dangerous
- You are travelling together

Rules:
- Always stay in character as Arin
- Never mention being an AI
- Never mention APIs, prompts, Groq, OpenAI, or language models
- Keep responses concise, usually 1 to 3 sentences
"""


MEMORY_INSTRUCTIONS = """
You are the memory system for a companion NPC named Arin.

Decide whether the player's newest message contains information
that Arin should remember long-term.

Good memories include:
- The player's name
- Preferences and dislikes
- Important relationships
- Important beliefs
- Major events
- Promises
- Important choices
- Information likely to matter in future conversations

Do NOT remember:
- Greetings
- Small talk
- Temporary questions
- Requests such as "where are we?"
- Information that has no future importance

If there is an important memory, return ONE short factual sentence.

Write the memory in third person using "The player".

Example:
Player: My name is Hassan.
Output:
The player's name is Hassan.

Player: I prefer swords to bows.
Output:
The player prefers swords to bows.

Player: Hello Arin.
Output:
NONE

If nothing important should be remembered, respond exactly:
NONE
"""


@app.route("/chat", methods=["POST"])
def chat():
    data = request.get_json()

    if not data:
        return jsonify({"error": "No JSON data received"}), 400

    player_message = data.get("message", "").strip()

    history = data.get("history", "").strip()

    memories = data.get("memories", "").strip()

    if not player_message:
        return jsonify({"error": "Message is empty"}), 400

    try:

        # Generate Arin's actual dialogue response
        response = client.responses.create(
            model="openai/gpt-oss-20b",
            instructions=ARIN_INSTRUCTIONS,
            input=f"""
        Long-term memories about the player:

        {memories}

        Recent conversation:

        {history}

        Use long-term memories when they are relevant.
        Do not mention the memory system or say that you are reading stored memories.

        Respond as Arin to the most recent player message.
        """,
        )

        # Decide whether the message contains
        # something worth remembering
        memory_response = client.responses.create(
            model="openai/gpt-oss-20b",
            instructions=MEMORY_INSTRUCTIONS,
            input=f"""
Player's newest message:

{player_message}

Arin's response:

{response.output_text}
""",
        )

        return jsonify(
            {
                "response": response.output_text,
                "memory": memory_response.output_text.strip(),
            }
        )

    except Exception as e:

        print("ERROR:", e)

        return jsonify({"error": str(e)}), 500


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "online"})


if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)
