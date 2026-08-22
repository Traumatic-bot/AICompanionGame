from flask import Flask, request, jsonify
from openai import OpenAI
from dotenv import load_dotenv
import os

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

@app.route("/chat", methods=["POST"])
def chat():
    data = request.get_json()

    if not data:
        return jsonify({"error": "No JSON data received"}), 400

    player_message = data.get("message", "").strip()
    history = data.get("history", "").strip()

    if not player_message:
        return jsonify({"error": "Message is empty"}), 400

    try:
        response = client.responses.create(
            model="openai/gpt-oss-20b",
            instructions=ARIN_INSTRUCTIONS,
            input=f"""
        Conversation so far:

        {history}

        Respond as Arin to the newest message.
        """
        )

        return jsonify({
            "response": response.output_text
        })

    except Exception as e:
        print("ERROR:", e)

        return jsonify({
            "error": str(e)
        }), 500


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "online"})


if __name__ == "__main__":
    app.run(
        host="127.0.0.1",
        port=5000,
        debug=True
    )