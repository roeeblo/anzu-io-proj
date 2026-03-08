const { SpawnItemCommand } = require('../../server/src/commands/SpawnItemCommand');
const { createMockWs } = require('../mocks/mockWs');

describe('SpawnItemCommand', () => {
  const defaultBounds = { minX: -20, maxX: 20, minY: -15, maxY: 15 };

  beforeAll(() => {
    console.log('[SpawnItemCommand] suite running');
  });

  describe('parseArgs', () => {
    it('returns { itemId, x, y } for valid "itemId x y"', () => {
      const cmd = new SpawnItemCommand('gem 0 0', null, defaultBounds);
      expect(cmd.parseArgs()).toEqual({ itemId: 'gem', x: 0, y: 0 });
    });

    it('returns itemId lowercased', () => {
      const cmd = new SpawnItemCommand('GEM 1 2', null, defaultBounds);
      expect(cmd.parseArgs()).toEqual({ itemId: 'gem', x: 1, y: 2 });
    });

    it('returns null when fewer than 3 parts', () => {
      const cmd = new SpawnItemCommand('gem 0', null, defaultBounds);
      expect(cmd.parseArgs()).toBeNull();
      expect(new SpawnItemCommand('gem', null, defaultBounds).parseArgs()).toBeNull();
    });

    it('returns null when x or y is not a number', () => {
      const cmd1 = new SpawnItemCommand('gem a b', null, defaultBounds);
      expect(cmd1.parseArgs()).toBeNull();
      const cmd2 = new SpawnItemCommand('gem 0 nan', null, defaultBounds);
      expect(cmd2.parseArgs()).toBeNull();
    });
  });

  describe('isWithinBounds', () => {
    it('returns true when x,y inside bounds', () => {
      const cmd = new SpawnItemCommand('', null, { minX: -10, maxX: 10, minY: -5, maxY: 5 });
      expect(cmd.isWithinBounds(0, 0)).toBe(true);
      expect(cmd.isWithinBounds(-10, -5)).toBe(true);
      expect(cmd.isWithinBounds(10, 5)).toBe(true);
    });

    it('returns false when x or y outside bounds', () => {
      const cmd = new SpawnItemCommand('', null, { minX: -10, maxX: 10, minY: -5, maxY: 5 });
      expect(cmd.isWithinBounds(-11, 0)).toBe(false);
      expect(cmd.isWithinBounds(11, 0)).toBe(false);
      expect(cmd.isWithinBounds(0, -6)).toBe(false);
      expect(cmd.isWithinBounds(0, 6)).toBe(false);
    });
  });

  describe('execute', () => {
    it('sends correct JSON when args valid and within bounds', async () => {
      const ws = createMockWs();
      const cmd = new SpawnItemCommand('gem 0 0', null, defaultBounds);
      const result = await cmd.execute(ws);
      expect(result).toBe(true);
      const sent = ws.getSentParsed();
      expect(sent).toHaveLength(1);
      expect(sent[0].type).toBe('SPAWN_ITEM');
      expect(sent[0].payload).toMatchObject({ itemId: 'gem', name: 'Gem', prefabType: 'gem', x: 0, y: 0 });
    });

    it('returns false when parseArgs returns null', async () => {
      const ws = createMockWs();
      const cmd = new SpawnItemCommand('gem 0', null, defaultBounds);
      const result = await cmd.execute(ws);
      expect(result).toBe(false);
      expect(ws.getSent()).toHaveLength(0);
    });

    it('returns false when out of bounds', async () => {
      const ws = createMockWs();
      const cmd = new SpawnItemCommand('gem 100 100', null, defaultBounds);
      const result = await cmd.execute(ws);
      expect(result).toBe(false);
      expect(ws.getSent()).toHaveLength(0);
    });
  });
});
