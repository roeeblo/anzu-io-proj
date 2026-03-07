const CommandFactory = require('../../server/src/commands/CommandFactory');

describe('CommandFactory', () => {
  describe('create', () => {
    it('returns MoveLeftCommand for MOVE_LEFT', () => {
      const cmd = CommandFactory.create('MOVE_LEFT');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('MOVE_LEFT');
    });
    it('returns MoveRightCommand for MOVE_RIGHT', () => {
      const cmd = CommandFactory.create('MOVE_RIGHT');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('MOVE_RIGHT');
    });
    it('returns StopCommand for STOP', () => {
      const cmd = CommandFactory.create('STOP');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('STOP');
    });
    it('returns JumpCommand for JUMP', () => {
      const cmd = CommandFactory.create('JUMP');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('JUMP');
    });
    it('returns AddScoreCommand for ADD_SCORE with amount arg', () => {
      const cmd = CommandFactory.create('ADD_SCORE', '10');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('ADD_SCORE');
    });
    it('returns SpawnItemCommand for SPAWN_ITEM with arg, db, bounds', () => {
      const bounds = { minX: -10, maxX: 10, minY: -10, maxY: 10 };
      const cmd = CommandFactory.create('SPAWN_ITEM', 'gem 0 0', null, bounds);
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('SPAWN_ITEM');
    });
    it('returns null for unknown type', () => {
      expect(CommandFactory.create('UNKNOWN')).toBeNull();
      expect(CommandFactory.create('')).toBeNull();
      expect(CommandFactory.create(null)).toBeNull();
    });
    it('normalizes type to uppercase', () => {
      const cmd = CommandFactory.create('move_left');
      expect(cmd).not.toBeNull();
      expect(cmd.type).toBe('MOVE_LEFT');
    });
  });
  describe('getValidTypes', () => {
    it('returns array including MOVE_LEFT, SPAWN_ITEM, ADD_SCORE', () => {
      const types = CommandFactory.getValidTypes();
      expect(Array.isArray(types)).toBe(true);
      expect(types).toContain('MOVE_LEFT');
      expect(types).toContain('ADD_SCORE amount');
      expect(types.some((t) => t.startsWith('SPAWN_ITEM'))).toBe(true);
    });
  });
});
