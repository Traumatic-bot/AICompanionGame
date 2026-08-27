# AI Companion Project – Development Documentation

## 1. Project Overview

The project was developed to investigate how a Large Language Model can be used to create a more intelligent companion NPC in a game.

The companion, Arin, was designed to do more than provide fixed dialogue. The aim was for the NPC to:

- respond naturally to the player
- maintain a consistent personality
- remember important information
- understand basic information about the current game situation
- react to gameplay events
- behave like a normal companion inside Unity

The development was completed incrementally. The basic Unity character and dialogue system were created first, followed by communication with the language model, memory, game context, proactive reactions, movement and animation.

---

# 2. Overall Architecture

The prototype contains two main parts:

```text
UNITY GAME
     |
     | HTTP
     v
PYTHON FLASK SERVER
     |
     | API request
     v
LANGUAGE MODEL
```

Unity handles the actual game.

The Python server handles communication between Unity and the external language model.

This separation was used so API credentials and LLM-specific code did not need to be placed directly inside the Unity game.

---

# 3. Stage 1 – Creating the Basic Unity Prototype

The first stage was creating a simple playable Unity scene.

The scene contained:

- a player character
- a first-person camera
- a companion NPC
- a basic environment
- user interface elements

`PlayerMovement.cs` was used to allow the player to move using the keyboard and control the camera using the mouse.

A `CharacterController` was used for player movement.

The purpose of this stage was simply to create a working environment in which the companion system could later be tested.

---

# 4. Stage 2 – NPC Interaction

A basic interaction system was then implemented.

`PlayerInteraction.cs` performs a raycast from the player's camera.

When the raycast detects an NPC that can be interacted with, the game displays:

```text
Press E to Talk
```

Pressing E opens the dialogue system.

Using a raycast means the player must be close enough and looking toward the NPC rather than being able to start a conversation from anywhere in the level.

The interaction system was later adjusted to search for `NPCInteraction` on the parent object as well. This allowed colliders placed on parts of the character model to still trigger interaction correctly.

---

# 5. Stage 3 – Dialogue User Interface

A dialogue interface was created using Unity UI and TextMeshPro.

The interface contains:

- dialogue history
- text input field
- Send button
- scrolling dialogue area

`DialogueManager.cs` controls the dialogue window.

When dialogue is opened:

- the player can type a message
- normal player movement is restricted
- the mouse cursor becomes available

When dialogue is closed:

- normal gameplay control returns
- the cursor is restored to gameplay mode

Escape can be used to close the dialogue.

Automatic scrolling was added so the newest dialogue remains visible.

---

# 6. Stage 4 – Python Backend

A Python backend was created inside the project:

```text
Backend/
```

The server uses Flask.

Its purpose is to receive requests from Unity, send them to the language model and return the response.

The server runs locally at:

```text
http://127.0.0.1:5000
```

The main dialogue endpoint is:

```text
POST /chat
```

A basic health endpoint is also available:

```text
GET /health
```

A batch file named `run.bat` was created to simplify starting the backend.

Example:

```bat
@echo off
cd /d "%~dp0"
call .venv\Scripts\activate
python server.py
pause
```

This changes to the backend directory, activates the Python virtual environment and starts the server.

---

# 7. Stage 5 – Connecting the Language Model

The backend uses an OpenAI-compatible Python client configured to communicate with Groq.

Conceptually, the setup is:

```python
client = OpenAI(
    api_key=os.environ.get("GROQ_API_KEY"),
    base_url="https://api.groq.com/openai/v1"
)
```

The API key is stored in:

```text
Backend/.env
```

rather than directly inside the code.

The environment file is excluded from Git.

The prototype uses:

```text
openai/gpt-oss-20b
```

as the language model.

---

# 8. Stage 6 – Creating Arin's Character Instructions

The backend contains a set of instructions describing how Arin should behave.

The instructions establish characteristics such as:

- loyal
- cautious
- slightly sarcastic
- practical
- protective of the player

They also instruct Arin to:

- remain in character
- avoid discussing the underlying AI system
- respond naturally
- keep most responses reasonably concise
- treat supplied game context as factual

This system instruction gives the language model a stable role instead of asking it to invent a new personality for every message.

---

# 9. Stage 7 – Sending Dialogue from Unity

When the player submits a message, `DialogueManager.cs` creates a request for the backend.

The request includes information similar to:

```text
message
history
memories
gameContext
```

This means the model is not only given the newest message.

It can also see:

- what has already been discussed
- important information remembered from earlier
- what is currently happening in the game

Unity sends the data to:

```text
http://127.0.0.1:5000/chat
```

The server returns JSON containing Arin's response.

Unity then displays the response in the dialogue window.

---

# 10. Stage 8 – Short-Term Conversation History

A conversation history system was added so Arin could refer to previous parts of the same discussion.

Without history, every request would effectively be an isolated conversation.

The dialogue manager therefore keeps a list of previous messages and supplies them to the backend.

Conversation history is also saved to a JSON file.

When the game starts again, the previous saved conversation can be loaded.

A development context-menu command was added to clear conversation history when testing.

---

# 11. Stage 9 – Persistent Long-Term Memory

Short-term history alone is not suitable for permanent information.

For example, if the player says:

```text
My brother is called Daniel.
```

that information could be useful later even if the complete conversation is no longer being supplied to the model.

`MemoryManager.cs` was therefore created.

It maintains a separate list of long-term memories and saves them to:

```text
memories.json
```

The main operations are:

```text
AddMemory
ReplaceMemory
GetMemoriesAsText
Save
Load
```

---

# 12. Stage 10 – Intelligent Memory Selection

The prototype does not save every player message as a memory.

After generating Arin's response, the backend makes another LLM request asking whether the conversation contains information worth storing.

The model returns one of three actions:

```text
ADD
REPLACE
NONE
```

Example ADD:

```json
{
  "action": "ADD",
  "old_memory": "",
  "new_memory": "The player prefers travelling at night."
}
```

Example REPLACE:

```json
{
  "action": "REPLACE",
  "old_memory": "The player is searching for the village.",
  "new_memory": "The player has found the village."
}
```

If no useful memory exists:

```json
{
  "action": "NONE",
  "old_memory": "",
  "new_memory": ""
}
```

Unity reads the memory action returned by the backend and updates `MemoryManager`.

This creates a simple form of persistent NPC memory.

---

# 13. Stage 11 – Saving Memory Separately from Conversation

Long-term memories and dialogue history are intentionally stored separately.

```text
conversation.json
memories.json
```

This was useful during testing.

For example:

1. Tell Arin a fact.
2. Allow the memory system to store it.
3. Clear conversation history.
4. Start another conversation.
5. Check whether Arin still knows the fact.

If the fact remains available, this demonstrates that the information is being retrieved from persistent memory rather than simply remaining inside conversation history.

---

# 14. Stage 12 – Dynamic Game Context

The next development stage was making Arin aware of the game rather than only the conversation.

`GameContextManager.cs` was created.

It stores values such as:

```text
Current location
Time of day
Player health
Danger nearby
Current objective
```

`GetGameContext()` converts this information into text for the language model.

Example:

```text
Current location: Old Forest
Time of day: Night
Player health: 35/100
Danger nearby: True
Current objective: Reach the ancient gate
```

This is included with every appropriate LLM request.

---

# 15. Updating Context from Gameplay

Several small Unity components can modify the current game context.

### LocationTrigger.cs

Changes the player's current location when a trigger is entered.

### DangerZone.cs

Marks whether danger is nearby.

### ObjectiveTrigger.cs

Changes the current objective.

### PlayerHealth.cs

Tracks the player's health.

### DamageZone.cs

Applies damage when the player enters a damaging area.

Together these scripts allow normal gameplay systems to update the information supplied to Arin.

---

# 16. Why Unity Controls the Game State

A deliberate design decision was made not to allow the LLM to become the authority over game rules.

For example, the language model cannot simply decide:

```text
The player now has 100 health.
```

and change the game.

Instead:

```text
Unity = source of truth
LLM = interpretation and dialogue
```

Unity decides:

- health
- objectives
- locations
- movement
- triggers
- progression

The model receives that information and responds appropriately.

This makes the AI companion easier to control and reduces conflicts between generated dialogue and actual gameplay.

---

# 17. Stage 13 – Proactive Companion Reactions

Normal dialogue requires the player to initiate conversation.

To make Arin feel more like a companion, a proactive reaction system was added.

`CompanionReactionManager.cs` can send a gameplay event to:

```text
POST /react
```

The request contains:

```text
gameEvent
gameContext
memories
```

The backend asks the language model to produce one short in-character reaction.

The reaction appears as a subtitle without opening the full dialogue interface.

Example:

```text
Arin: I'd stay away from that fire if I were you.
```

This system can be triggered by events such as:

- entering danger
- low player health
- discovering an area
- reaching an objective

---

# 18. Stage 14 – Humanoid Companion Model

The original placeholder companion was replaced with a humanoid character.

The model was imported into Unity and configured using the Humanoid rig system.

The main character model provides the avatar.

Separate animation files use:

```text
Copy From Other Avatar
```

and reference the companion's avatar.

This allows separate animation clips to control the same character skeleton.

---

# 19. Stage 15 – Animation Controller

An Animator Controller was created with:

```text
Idle
Walking
Talking
```

Two Boolean parameters are used:

```text
IsWalking
IsTalking
```

The walking state is controlled by the companion's movement velocity.

The talking state is controlled by whether dialogue is currently open.

Typical logic:

```text
Idle -> Walking
Walking -> Idle

Idle -> Talking
Walking -> Talking
Talking -> Idle
```

This prevents the talking animation from constantly restarting.

Root motion is disabled because movement is controlled by the NavMeshAgent rather than by the animation itself.

---

# 20. Stage 16 – Companion Navigation

`CompanionFollow.cs` uses a Unity `NavMeshAgent`.

The companion:

1. measures the distance to the player
2. follows if the player is too far away
3. stops when within the configured follow distance
4. changes animation according to movement
5. stops navigation while dialogue is open
6. rotates to face the player during conversation

A NavMesh was baked for the playable environment.

This allows Arin to move around obstacles rather than simply moving directly toward the player's position.

---

# 21. Main Unity Components

## DialogueManager

Responsible for:

- dialogue UI
- HTTP dialogue requests
- displaying responses
- conversation history
- save/load of dialogue
- forwarding memory and context

## MemoryManager

Responsible for:

- long-term memory list
- adding memories
- replacing memories
- saving/loading memory

## GameContextManager

Responsible for:

- current gameplay context
- producing context information for the language model

## CompanionReactionManager

Responsible for:

- event-based LLM requests
- displaying short proactive subtitles

## CompanionFollow

Responsible for:

- following the player
- stopping near the player
- facing the player
- updating companion animation states

## PlayerInteraction / NPCInteraction

Responsible for:

- detecting the companion
- interaction prompt
- starting dialogue

## PlayerHealth

Responsible for:

- player health
- damage/healing
- exposing health information to game context

---

# 22. Data Flow Example

Suppose the player is in a dangerous forest and says:

```text
Player: Do you think we should keep going?
```

Unity may currently hold:

```text
Location: Dark Forest
Health: 42/100
Danger nearby: True
Objective: Find the gate
```

It sends:

```text
Player message
+ conversation history
+ long-term memories
+ game context
```

to the backend.

The backend also adds Arin's character instructions.

The model might then produce:

```text
Arin: We can keep moving, but you're hurt and this place isn't exactly welcoming. Stay close.
```

The important point is that the response can reference both the conversation and actual game state.

---

# 23. Local Save System

The prototype uses:

```csharp
Application.persistentDataPath
```

for persistent files.

The main files are:

```text
conversation.json
memories.json
```

This keeps prototype persistence simple and avoids requiring a database.

---

# 24. Development and Testing Approach

The system was developed in small independent stages.

A typical pattern was:

```text
Create feature
    |
Test feature independently
    |
Fix problems
    |
Connect it to previous system
    |
Save working version in Git
```

Examples include:

- testing dialogue before memory
- testing memory before game context
- testing context before proactive reactions
- testing the humanoid model before navigation animations
- testing NavMesh following before connecting talking behaviour

This incremental approach made debugging easier because only a small number of new components were introduced at each stage.

---

# 25. Git Version Control

Git was used to save working development checkpoints.

Generated or sensitive files were excluded using `.gitignore`, including:

```text
Library/
Temp/
Backend/.venv/
Backend/.env
__pycache__/
*.pyc
```

This keeps the repository smaller and prevents the API key from being committed.

---

# 26. What the Prototype Demonstrates

The completed prototype combines several AI companion concepts:

### Natural Language Interaction
The player can type normal messages rather than selecting only predefined dialogue options.

### Character Consistency
System instructions establish Arin's intended personality.

### Short-Term Context
Conversation history allows dialogue to remain coherent.

### Persistent Memory
Important information can survive beyond the active conversation.

### World Awareness
The model receives factual game state from Unity.

### Proactive Behaviour
Arin can comment on events without waiting for a dialogue prompt.

### Physical Game Integration
Arin follows the player using navigation and responds visually using animations.

---

# 27. Limitations

The prototype has several limitations which would be relevant for evaluation.

### Network dependency

The language model currently requires an external service.

### Latency

Generated dialogue is slower than traditional predefined dialogue.

### Memory reliability

Memory selection is performed by an LLM, meaning some memories may be unnecessary or information may occasionally be missed.

### Simple retrieval

All stored memories can be supplied as text. A larger game would need a more selective retrieval system.

### Limited story-state control

A larger narrative game would require stricter rules defining what an NPC is allowed to know at each point in the story.

### Backend dependency

The local Flask backend must currently be running while testing the AI functionality.

---

# 28. Suitable Future Development

Possible extensions include:

- separate memory stores for different NPCs
- relationship values
- emotional state
- story progression flags
- NPC-specific knowledge restrictions
- semantic memory retrieval
- automatic memory summarisation
- combat awareness
- voice recognition
- text-to-speech
- streamed dialogue
- more advanced behaviour trees
- automatic evaluation logging
- configurable model providers
- local/offline language models

---

# 29. Summary

The prototype was developed by combining a conventional Unity companion system with an LLM-powered dialogue layer.

Unity remains responsible for the actual game world, while the language model is supplied with information about that world and generates appropriate natural-language behaviour.

The final system can therefore be summarised as:

```text
Player interacts with Arin
        |
Unity gathers relevant information
        |
Dialogue + memory + game context
        |
Local Flask backend
        |
Language model generates response
        |
Unity displays response
        |
Important information may be saved
```

This provides a practical example of how an LLM can enhance a game companion without replacing the deterministic systems required to run the game itself.
