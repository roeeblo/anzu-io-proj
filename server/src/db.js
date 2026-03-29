const { MongoClient } = require('mongodb');

const uri = process.env.MONGODB_URI || 'mongodb://localhost:27017';
const dbName = process.env.MONGODB_DB || 'socketrunner';

let _client;
let _db;

const spawnablesSeed = [
  { itemId: 'gem', name: 'Gem', prefabType: 'gem' },
  { itemId: 'cherry', name: 'Cherry', prefabType: 'cherry' },
];

async function connect() {
  if (_db) return _db;
  const connectTimeoutMS = Number(process.env.MONGODB_CONNECT_TIMEOUT_MS) || 5000;
  const serverSelectionTimeoutMS = Number(process.env.MONGODB_SERVER_SELECTION_TIMEOUT_MS) || 5000;
  const options = { connectTimeoutMS, serverSelectionTimeoutMS };
  _client = new MongoClient(uri, options);
  await _client.connect();
  _db = _client.db(dbName);
  await seedSpawnables();
  return _db;
}

async function seedSpawnables() {
  const col = _db.collection('spawnables');
  for (const doc of spawnablesSeed) {
    await col.updateOne(
      { itemId: doc.itemId },
      { $set: doc },
      { upsert: true }
    );
  }
  if (process.env.NODE_ENV !== 'test') console.log('[DB] Spawnables: gem, cherry');
}

function getDb() {
  return _db;
}

async function close() {
  if (_client) {
    await _client.close();
    _client = null;
    _db = null;
  }
}

module.exports = { connect, getDb, close };
