const { Command } = require('./Command');

class MoveLeftCommand extends Command {
  get type() {
    return 'MOVE_LEFT';
  }
}

module.exports = { MoveLeftCommand };
