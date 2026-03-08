const { AddScoreCommand } = require('../../server/src/commands/AddScoreCommand');
const { createMockWs } = require('../mocks/mockWs');

describe('AddScoreCommand', () => {
  beforeAll(() => {
    console.log('[AddScoreCommand] suite running');
  });
  it('execute(ws) sends JSON with type ADD_SCORE and payload.amount from constructor', () => {
    const ws = createMockWs();
    const cmd = new AddScoreCommand('5');
    const result = cmd.execute(ws);
    expect(result).toBe(true);
    const sent = ws.getSentParsed();
    expect(sent).toHaveLength(1);
    expect(sent[0].type).toBe('ADD_SCORE');
    expect(sent[0].payload).toEqual({ amount: 5 });
  });

  it('defaults amount to 1 when arg is invalid or negative', () => {
    const ws = createMockWs();
    const cmd = new AddScoreCommand('invalid');
    cmd.execute(ws);
    expect(ws.getSentParsed()[0].payload.amount).toBe(1);

    ws.reset();
    const cmd2 = new AddScoreCommand('-1');
    cmd2.execute(ws);
    expect(ws.getSentParsed()[0].payload.amount).toBe(1);
  });

  it('caps amount at 999', () => {
    const ws = createMockWs();
    const cmd = new AddScoreCommand('9999');
    cmd.execute(ws);
    expect(ws.getSentParsed()[0].payload.amount).toBe(999);
  });

  it('returns false when ws is null or not OPEN', () => {
    const cmd = new AddScoreCommand('5');
    expect(cmd.execute(null)).toBe(false);

    const closedWs = createMockWs({ readyState: 3 });
    expect(cmd.execute(closedWs)).toBe(false);
    expect(closedWs.getSent()).toHaveLength(0);
  });
});
