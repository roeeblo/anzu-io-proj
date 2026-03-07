class Command {
  get type() {
    throw new Error('Subclass must define type');
  }

  execute(ws) {
    if (!ws || ws.readyState !== 1) return;
    ws.send(JSON.stringify({ type: this.type }));
  }
}

module.exports = { Command };
