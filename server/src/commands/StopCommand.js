const { Command } = require('./Command');

class StopCommand extends Command {
  get type() {
    return 'STOP';
  }
}

module.exports = { StopCommand };
