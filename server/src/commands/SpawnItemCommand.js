const { Command } = require('./Command');

const USAGE = 'Usage: SPAWN_ITEM itemId x y  (e.g. SPAWN_ITEM gem 0 0). itemId: gem, cherry';

const FALLBACK_ITEMS = {
  gem: { itemId: 'gem', name: 'Gem', prefabType: 'gem' },
  cherry: { itemId: 'cherry', name: 'Cherry', prefabType: 'cherry' },
};

class SpawnItemCommand extends Command {
  constructor(arg, db, bounds) {
    super();
    this._arg = arg || '';
    this._db = db;
    this._bounds = bounds || { minX: -20, maxX: 20, minY: -15, maxY: 15 };
  }

  get type() {
    return 'SPAWN_ITEM';
  }

  parseArgs() {
    const parts = this._arg.trim().split(/\s+/).filter(Boolean);
    if (parts.length < 3) return null;

    const itemId = parts[0].toLowerCase();
    const x = parseFloat(parts[1]);
    const y = parseFloat(parts[2]);

    if (Number.isNaN(x) || Number.isNaN(y)) return null;

    return { itemId, x, y };
  }

  isWithinBounds(x, y) {
    const b = this._bounds;
    return x >= b.minX && x <= b.maxX && y >= b.minY && y <= b.maxY;
  }

  async getSpawnable(itemId) {
    let doc = FALLBACK_ITEMS[itemId];

    if (!this._db) return doc;

    try {
      const fromDb = await this._db.collection('spawnables').findOne({ itemId });
      if (fromDb) return fromDb;
    } catch (err) {
      process.stderr.write('DB lookup failed, using fallback\n');
    }

    return doc;
  }

  async execute(ws) {
    if (!ws || ws.readyState !== 1) return true;

    const parsed = this.parseArgs();
    if (!parsed) {
      process.stderr.write(USAGE + '\n');
      return false;
    }

    const { itemId, x, y } = parsed;

    if (!this.isWithinBounds(x, y)) {
      const b = this._bounds;
      process.stderr.write(
        'Cannot spawn - out of bounds. Try again. (X: ' +
          b.minX + '-' + b.maxX +
          ', Y: ' + b.minY + '-' + b.maxY + ')\n'
      );
      return false;
    }

    const doc = await this.getSpawnable(itemId);
    if (!doc) {
      process.stderr.write('Unknown itemId: "' + itemId + '". Use: gem, cherry\n');
      return false;
    }

    ws.send(JSON.stringify({
      type: this.type,
      payload: {
        itemId: doc.itemId,
        name: doc.name,
        prefabType: doc.prefabType,
        x,
        y,
      },
    }));

    return true;
  }
}

module.exports = { SpawnItemCommand };
