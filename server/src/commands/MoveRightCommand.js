const { Command } = require('./Command');

class MoveRightCommand extends Command {
  get type() {
    return 'MOVE_RIGHT';
  }
}

module.exports = { MoveRightCommand };
