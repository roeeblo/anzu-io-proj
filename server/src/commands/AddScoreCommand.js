const { Command } = require('./Command');

class AddScoreCommand extends Command {
  constructor(arg) {
    super();
    const n = parseInt(arg, 10);
    this._amount = Number.isNaN(n) || n < 0 ? 1 : Math.min(n, 999);
  }

  get type() {
    return 'ADD_SCORE';
  }

  execute(ws) {
    if (!ws || ws.readyState !== 1) return false;
    ws.send(JSON.stringify({ type: this.type, payload: { amount: this._amount } }));
    return true;
  }
}

module.exports = { AddScoreCommand };
