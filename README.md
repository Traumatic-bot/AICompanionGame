# AICompanionGame
# Intelligent Companion NPC Using Large Language Models

A Unity prototype that demonstrates an AI-driven companion NPC capable of natural conversation, persistent memory, awareness of game context, and proactive reactions to gameplay events.

The project was developed as a university final-year project focused on integrating a Large Language Model (LLM) into a game companion while keeping the game logic controlled by Unity.

---

## Main Features

- Natural conversation with the companion NPC, **Arin**
- LLM-generated replies through a local Python backend
- Short-term conversation history
- Persistent long-term memories
- Dynamic awareness of:
  - current location
  - time of day
  - player health
  - nearby danger
  - current objective
- Proactive companion reactions without opening the dialogue window
- Humanoid animated companion
- NavMesh-based following
- Idle, walking and talking animations
- Interaction prompt and dialogue UI
- Local save files for conversation and memories

---

## Technology Used

### Unity
- Unity 6
- C#
- Universal Render Pipeline (URP)
- Unity Input System
- AI Navigation / NavMesh
- TextMeshPro

### Backend
- Python
- Flask
- OpenAI-compatible Python SDK
- Groq API
- `openai/gpt-oss-20b`

Unity does not send requests directly to the external LLM provider. Instead, Unity communicates with a small local Flask server.

---

## How the System Works

```text
Player
  |
  v
Unity Dialogue System
  |
  | HTTP request
  v
Local Flask Backend
  |
  | LLM request
  v
Groq-hosted language model
  |
  v
Flask Backend
  |
  | JSON response
  v
Unity
  |
  +--> Displays Arin's reply
  |
  +--> Updates long-term memory when required
```

When the player talks to Arin, Unity sends:

- the player's new message
- recent conversation history
- saved long-term memories
- current game context

The backend combines this information with Arin's character instructions and asks the language model to produce an in-character response.

A second model request determines whether anything important should be added to or updated in long-term memory.

---

## Project Structure

The exact asset layout may vary slightly, but the main components are:

```text
AICompanionGame/
|
|-- Assets/
|   |-- Scenes/
|   |-- Scripts/
|   |-- Models/
|   |-- Materials/
|   `-- UI/
|
|-- Backend/
|   |-- server.py
|   |-- run.bat
|   |-- .env
|   `-- .venv/
|
`-- README.md
```

Important Unity scripts include:

```text
DialogueManager.cs
MemoryManager.cs
GameContextManager.cs
CompanionReactionManager.cs
CompanionFollow.cs
NPCInteraction.cs
PlayerInteraction.cs
PlayerMovement.cs
PlayerHealth.cs
LocationTrigger.cs
ObjectiveTrigger.cs
DangerZone.cs
DamageZone.cs
```

---

# Setup

## 1. Open the Unity project

Open the project using the Unity version it was developed with.

Allow Unity to import the project completely before entering Play Mode.

---

## 2. Configure the backend

Open:

```text
Backend/.env
```

Add a Groq API key:

```env
GROQ_API_KEY=your_api_key_here
```

Do not commit the real API key to Git.

The `.gitignore` should exclude:

```text
Backend/.env
Backend/.venv/
__pycache__/
*.pyc
```

---

## 3. Start the Python backend

The easiest method is:

```text
Backend/run.bat
```

This activates the virtual environment and starts `server.py`.

The backend normally runs at:

```text
http://127.0.0.1:5000
```

A health endpoint is available at:

```text
/health
```

---

## 4. Start Unity

With the backend running:

1. Open the gameplay scene.
2. Press **Play**.
3. Walk close to Arin.
4. Look at the NPC.
5. Press **E** when the interaction prompt appears.
6. Enter a message and press Send.
7. Arin should reply through the dialogue UI.

Press **Escape** to close dialogue and return control to the player.

---

# Companion Behaviour

## Following

`CompanionFollow.cs` uses a `NavMeshAgent`.

Arin follows the player when outside the configured follow distance and stops when close enough.

The script also updates animation states based on whether Arin is:

- walking
- idle
- talking

While dialogue is open, Arin stops following and turns toward the player.

---

## Dialogue

`DialogueManager.cs` is responsible for:

- opening and closing the dialogue UI
- receiving player messages
- sending HTTP requests to the backend
- displaying Arin's responses
- maintaining conversation history
- saving and loading conversations
- passing long-term memory and game context to the backend

The backend `/chat` endpoint returns the reply together with any requested memory operation.

---

## Long-Term Memory

`MemoryManager.cs` stores important information separately from normal conversation history.

Example:

```text
Player: My name is Alex.
```

The model may decide this information is worth remembering.

A memory operation can be:

```text
ADD
REPLACE
NONE
```

Memories are saved locally and loaded again when the game starts.

This allows Arin to remember important information even if the normal conversation history is cleared.

---

## Game Context

`GameContextManager.cs` creates a text description of the current game state.

Example:

```text
Current location: Forest
Time of day: Night
Player health: 40/100
Danger nearby: True
Current objective: Find the gate
```

This context is sent with dialogue requests, allowing Arin to respond to what is happening in the game rather than only reacting to typed messages.

Other Unity scripts update this context when gameplay changes.

---

## Proactive Reactions

The companion can also speak without the player opening dialogue.

`CompanionReactionManager.cs` sends a gameplay event to the backend `/react` endpoint.

Example events could include:

```text
The player entered a dangerous area.
The player is badly injured.
The player discovered a new location.
```

The backend generates a short in-character reaction which appears as a subtitle.

This makes the companion feel more connected to gameplay.

---

# Save Data

Conversation and memory data are stored using Unity's:

```csharp
Application.persistentDataPath
```

Typical files include:

```text
conversation.json
memories.json
```

The exact physical folder depends on the operating system and Unity player settings.

---

# Testing

Useful tests for the prototype include:

### Conversation
- Arin responds to normal questions.
- Responses remain concise and in character.
- Dialogue closes correctly with Escape.

### Memory
1. Tell Arin an important personal fact.
2. Allow it to be stored as long-term memory.
3. Clear the normal conversation history.
4. Talk to Arin again.
5. Check whether the remembered fact is still available.

### Context
- Enter a location trigger and ask Arin where you are.
- Change the current objective and ask what should be done next.
- Enter a danger zone and check the contextual response.
- Damage the player and check whether health is reflected correctly.

### Companion movement
- Arin follows the player.
- Arin stops within the configured follow distance.
- Walking animation only plays while moving.
- Talking animation plays during dialogue.
- Arin faces the player during conversation.

---

# Important Design Decision

The LLM controls **dialogue**, not the authoritative game state.

Unity remains responsible for:

- player health
- locations
- objectives
- movement
- triggers
- progression
- combat/gameplay rules

The current game state is supplied to the LLM as factual context.

This prevents the language model from changing important game systems simply by inventing information in dialogue.

---

# Known Limitations

This is a prototype and not a production-ready AI system.

Current limitations include:

- requires an internet connection for LLM responses
- requires the Python backend to be running
- response time depends on network/model latency
- memory extraction is model-based and may occasionally select an unnecessary memory
- saved memory is currently relatively simple
- context is provided as text rather than a more complex structured world model
- dialogue safety and narrative restrictions could be expanded
- additional NPCs would benefit from separate identities, knowledge and memory stores

---

# Possible Future Improvements

- per-NPC memory
- story-state-aware knowledge restrictions
- relationship/trust system
- emotion or mood state
- companion combat assistance
- better memory ranking and retrieval
- memory summaries
- structured JSON game context
- streaming responses
- voice input and text-to-speech
- dialogue evaluation tools
- automated logging for user studies
- configurable LLM providers
- offline/local language model support

---

## Purpose

The prototype demonstrates how an LLM can be integrated into a traditional game architecture to create a companion that can:

- communicate naturally
- remember information
- understand changing game context
- react to gameplay events
- retain a consistent character identity

The main principle is that the AI enhances the NPC's behaviour while the game engine remains in control of the actual game world.
