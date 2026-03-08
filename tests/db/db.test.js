const path = require('path');
const dbPath = path.resolve(__dirname, '../../server/src/db.js');

let mongod;
let dbModule;
let savedEnv;

beforeAll(async () => {
  savedEnv = { MONGODB_URI: process.env.MONGODB_URI, MONGODB_DB: process.env.MONGODB_DB };
  const { MongoMemoryServer } = require('mongodb-memory-server');
  mongod = await MongoMemoryServer.create();
  process.env.MONGODB_URI = mongod.getUri();
  process.env.MONGODB_DB = 'anzu_test';
  delete require.cache[dbPath];
  dbModule = require(dbPath);
  await dbModule.connect();
}, 15000);

afterAll(async () => {
  if (dbModule && dbModule.close) await dbModule.close();
  if (mongod) await mongod.stop();
  if (savedEnv) {
    process.env.MONGODB_URI = savedEnv.MONGODB_URI;
    process.env.MONGODB_DB = savedEnv.MONGODB_DB;
  }
  delete require.cache[dbPath];
});

describe('DB', () => {
  beforeAll(() => {
    console.log('[DB] suite running');
  });
  it('connect() returns db and getDb() returns same', async () => {
    const d = dbModule.getDb();
    expect(d).not.toBeNull();
    expect(d.databaseName).toBe('anzu_test');
  });

  it('seedSpawnables creates gem and cherry', async () => {
    const col = dbModule.getDb().collection('spawnables');
    const gem = await col.findOne({ itemId: 'gem' });
    const cherry = await col.findOne({ itemId: 'cherry' });
    expect(gem).toMatchObject({ itemId: 'gem', name: 'Gem', prefabType: 'gem' });
    expect(cherry).toMatchObject({ itemId: 'cherry', name: 'Cherry', prefabType: 'cherry' });
  });

  it('SpawnItemCommand getSpawnable uses DB when connected', async () => {
    const { SpawnItemCommand } = require('../../server/src/commands/SpawnItemCommand');
    const cmd = new SpawnItemCommand('gem 0 0', dbModule.getDb(), { minX: -20, maxX: 20, minY: -15, maxY: 15 });
    const doc = await cmd.getSpawnable('gem');
    expect(doc).toMatchObject({ itemId: 'gem', name: 'Gem', prefabType: 'gem' });
  });
});
