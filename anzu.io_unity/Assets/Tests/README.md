# Unity Tests

Run via **Window → General → Test Runner** (Edit Mode and Play Mode).

## Structure

- **Unit/** – Edit Mode + Play Mode unit tests (CommandFactory, pure logic).
- **PlayMode/** – Tests that need runtime/physics (GameManager, PlayerController).

## Test files

| File | Description |
|------|-------------|
| `Unit/CommandFactoryTest.cs` | CommandFactory.Create: null, ADD_SCORE, MOVE_LEFT/RIGHT/STOP/JUMP/SPAWN_ITEM, unknown type. |
| `PlayMode/GameManagerScoreTest.cs` | GameManager: at 100 points GameWon is true; below 100 false. |
| `PlayMode/PlayerControllerTest.cs` | PlayerController: MoveLeft/MoveRight/Stop/Jump velocity; SpawnItem no throw. |
