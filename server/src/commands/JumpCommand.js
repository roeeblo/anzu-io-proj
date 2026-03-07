const { Command } = require('./Command');

class JumpCommand extends Command {
  get type() {
    return 'JUMP';
  }
}

module.exports = { JumpCommand };
