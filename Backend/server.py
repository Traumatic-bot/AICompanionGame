from flask import Flask, request, jsonify
from openai import OpenAI
from dotenv import load_dotenv
import os
import json

load_dotenv()

app = Flask(__name__)

client = OpenAI(
    api_key=os.environ.get("GROQ_API_KEY"),
    base_url="https://api.groq.com/openai/v1"
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
You manage the long-term memories of a companion NPC named Arin.

You will receive:
1. Arin's existing memories about the player.
2. The player's newest message.
3. Arin's response.

Decide whether the newest message should change long-term memory.

There are three possible actions:

ADD
Use when the player reveals an important new fact that is not already stored.

REPLACE
Use when the player changes, corrects, or contradicts an existing memory.

NONE
Use when nothing important needs to change.

Good memories include:
- Name
- Preferences and dislikes
- Relationships
- Important beliefs
- Major events
- Promises
- Important choices
- Information useful in future conversations

Do not remember greetings, small talk, or temporary questions.

Return ONLY valid JSON.

For a new memory:

{
    "action": "ADD",
    "old_memory": "",
    "new_memory": "The player prefers swords to bows."
}

For an updated memory:

{
    "action": "REPLACE",
    "old_memory": "The player's favourite food is chicken.",
    "new_memory": "The player's favourite food is pizza."
}

IMPORTANT:
For REPLACE, old_memory must copy the existing memory EXACTLY.

If nothing should change:

{
    "action": "NONE",
    "old_memory": "",
    "new_memory": ""
}
"""


@app.route("/chat", methods=["POST"])
def chat():
    data = request.get_json()

    if not data:
        return jsonify({
            "error": "No JSON data received"
        }), 400

    player_message = data.get(
        "message", ""
    ).strip()

    history = data.get(
        "history", ""
    ).strip()

    memories = data.get(
        "memories", ""
    ).strip()

    if not player_message:
        return jsonify({
            "error": "Message is empty"
        }), 400

    try:
        # Generate Arin's dialogue response
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
"""
        )

        # Decide whether long-term memory should change
        memory_response = client.responses.create(
            model="openai/gpt-oss-20b",
            instructions=MEMORY_INSTRUCTIONS,
            input=f"""
Existing long-term memories:

{memories}

Player's newest message:

{player_message}

Arin's response:

{response.output_text}
"""
        )

        memory_text = memory_response.output_text.strip()

        try:
            memory_data = json.loads(memory_text)

        except json.JSONDecodeError:
            print(
                "Could not parse memory JSON:",
                memory_text
            )

            memory_data = {
                "action": "NONE",
                "old_memory": "",
                "new_memory": ""
            }

        return jsonify({
            "response": response.output_text,

            "memoryAction":
                memory_data.get(
                    "action",
                    "NONE"
                ),

            "oldMemory":
                memory_data.get(
                    "old_memory",
                    ""
                ),

            "newMemory":
                memory_data.get(
                    "new_memory",
                    ""
                )
        })

    except Exception as e:
        print("ERROR:", e)

        return jsonify({
            "error": str(e)
        }), 500


@app.route("/health", methods=["GET"])
def health():
    return jsonify({
        "status": "online"
    })


if __name__ == "__main__":
    app.run(
        host="127.0.0.1",
        port=5000,
        debug=True
    )