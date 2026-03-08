## Anzu.io – Server–Unity Communication

This project focuses on the communication architecture between a Node.js server and a Unity client.  
The unity gameplay itself is intentionally minimal and not meant to be a fully designed game.

Node.js WebSocket server and Unity client that communicate through commands, with a browser controller for manual input.


## Features

- WebSocket handshake between server and Unity client.
- Command system (move, jump, stop, add score, spawn items).
- Browser UI to send commands (http://localhost:8080).
- Commands can also be sent from the server terminal.
- Unity executes commands and sends responses back to the server.
- Optional MongoDB support for spawnable items (gem, cherry).
- Tests with Jest and Unity Test Framework.
- CI with GitHub Actions and dependency updates with Dependabot.

## Screenshots
UI:
<img width="1055" height="879" alt="image" src="https://github.com/user-attachments/assets/a10de9b2-db62-47ff-8337-f4d9887a0266" />

Gameplay:
<img width="1079" height="734" alt="image" src="https://github.com/user-attachments/assets/6bc1b2ad-81af-42a5-a010-aa0a4e8b3d87" />



## Scalability

- DB layer

A database (MongoDB) is used for spawnable items even though it could work with an enum. This allows adding new items or data later without changing the architecture.

- Client identification in handshake

HANDSHAKE_ACK includes a client name (for example "unity" or "postman"). This allows the server to distinguish between client types and apply different behaviour if needed.

- CI pipeline & Docker Container

GitHub Actions runs the full test suite on every push and pull request to ensure new changes do not break the system, as well as contanirized the server with docker for easy use.

- Dependabot

Runs weekly and creates pull requests for dependency updates so libraries and security remain up to date.

- Logging and debug visibility

Additional comments and descriptive console prints were added across the server and tests to indicate success, failure, and flow. This helps track behaviour during tests and makes CI logs easier to read when something fails.

## Design patterns used

- Singleton  
GameManager in Unity is a single shared instance (GameManager.Instance). Score and win state are stored there so the whole game uses the same source of truth.

- Command  
Each action (move, jump, add score, etc) is represented by a command object with Execute() and a type. The server sends a command type and payload, Unity creates the correct command and executes it.

- Factory  
CommandFactory.Create(data, player) creates the correct command based on the message type. Adding a new command only requires creating a class and registering it in the factory.

## Commands

- MOVE_LEFT  
Player moves left.

- MOVE_RIGHT  
Player moves right.

- STOP  
Stops horizontal movement.

- JUMP  
Player jumps if grounded.

- ADD_SCORE  
Adds points to the score. The payload contains an amount. When the score reaches 100 the game sends GAME_OVER.

- SPAWN_ITEM  
Spawns an item (for example gem or cherry) at a position. Payload includes prefabType, x and y. The server validates bounds. Spawnables can come from MongoDB or built-in defaults.

## Handshake flow

- Server sends HANDSHAKE
- Client replies with HANDSHAKE_ACK including a client name (for example "unity")
- Unity sends COMMAND_DONE after executing a command
- Unity sends GAME_OVER when the player wins

## Requirements

- **Without Docker:** Node.js 18 or newer
- **With Docker:** Docker and Docker Compose
- Unity 2018 or newer (open the anzu.io_unity project)
- MongoDB optional (for spawnables; server runs without it)

## How to run

<<<<<<< HEAD
You can run the server **with Docker** or **without Docker** (local Node.js).
=======
WITHOUT DOCKER (Option 1):
![0308](https://github.com/user-attachments/assets/718aeb20-babb-42ce-9128-612c89dee646)


1. Clone the repository
2. Run the Node.js server
cd server  
npm install  
node src/app.js
>>>>>>> 09e7b34352634a867df5d604ec42bbc980dd9ba6

### Option A: Without Docker (local Node.js)

<<<<<<< HEAD
1. Clone the repository.
2. Run the Node.js server:
   ```bash
   cd server
   npm install
   node src/app.js
   ```
   The server will start on http://localhost:8080. Leave the terminal open.

### Option B: With Docker

From the repository root:

```bash
docker compose up --build
```

The server will start on http://localhost:8080. To run in the background: `docker compose up -d`.

To stop: `docker compose down`.

*(With Docker there is no interactive server terminal for typing commands; use the browser UI or run without Docker for that.)*

---

**Then (same for both options):**

3. Browser controller (optional, but recommended)
Open a browser and go to:
=======
3. Browser controller (optional, but recommended :) )
>>>>>>> 09e7b34352634a867df5d604ec42bbc980dd9ba6
http://localhost:8080

4. Commands from terminal (optional)
In the server terminal you can type commands directly:
MOVE_LEFT
MOVE_RIGHT
STOP
JUMP
ADD_SCORE
ADD_SCORE 50
SPAWN_ITEM gem 0 0
SPAWN_ITEM cherry 2 -1

5. Run the Unity client
Open Unity Hub → Open Project → select the anzu.io_unity folder.
Open the main scene and press Play.
Commands from the browser or terminal will now affect the game.

---

WITH DOCKER (Option 2):

From the project root run:
docker compose up --build

Keep the terminal open. Then open http://localhost:8080 in your browser

Leave the container running.

Then follow **steps 3–5 above** to open the browser controller and run the Unity client.

## Run order

- Start the server
- Press Play in Unity
- Send commands from browser or terminal

## Running tests

From the repository root:
npm install  
npm test

This runs all Jest tests.

## Unity tests

Open Unity → Window → General → Test Runner

Run:
- Edit Mode tests
- Play Mode tests

## Project structure

anzu-io-proj/

server/  
Node server (src/app.js, db.js, commands)  
public/ contains the browser UI (index.html)

anzu.io_unity/  
Unity client

tests/  
Jest tests (unit, integration, db, ui, mocks)

.github/  
CI workflow and Dependabot
