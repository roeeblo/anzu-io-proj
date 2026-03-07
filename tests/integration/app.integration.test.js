/**
 * Integration tests: real HTTP+WS server, multiple clients, message flow.
 * Run with NODE_ENV=test (Jest sets this). Server listens on port 0 (random).
 */
const WebSocket = require('ws');

function connectWs(url) {
  return new Promise((resolve, reject) => {
    const ws = new WebSocket(url);
    const messages = [];
    ws.on('message', (data) => messages.push(JSON.parse(data.toString())));
    ws.on('open', () => resolve({ ws, messages }));
    ws.on('error', reject);
  });
}

function send(ws, obj) {
  return new Promise((resolve, reject) => {
    ws.send(JSON.stringify(obj), (err) => (err ? reject(err) : resolve()));
  });
}

function waitForMessage(messages, type, timeoutMs = 2000) {
  return new Promise((resolve, reject) => {
    const deadline = Date.now() + timeoutMs;
    const check = () => {
      const idx = messages.findIndex((m) => m.type === type);
      if (idx >= 0) return resolve(messages[idx]);
      if (Date.now() > deadline) return reject(new Error('Timeout waiting for ' + type));
      setImmediate(check);
    };
    check();
  });
}

describe('App integration', () => {
  let server;
  let port;
  const originalEnv = process.env.NODE_ENV;

  beforeAll(async () => {
    process.env.NODE_ENV = 'test';
    const { start } = require('../../server/src/app');
    server = await start(0);
    port = server.address().port;
  }, 10000);

  afterAll(async () => {
    process.env.NODE_ENV = originalEnv;
    if (server) await new Promise((resolve) => server.close(resolve));
    const { close: closeDb } = require('../../server/src/db');
    await closeDb().catch(() => {});
  });

  it('sends HANDSHAKE on connect', async () => {
    const { ws, messages } = await connectWs('ws://127.0.0.1:' + port);
    await waitForMessage(messages, 'HANDSHAKE');
    expect(messages.some((m) => m.type === 'HANDSHAKE')).toBe(true);
    expect(messages.find((m) => m.type === 'HANDSHAKE').payload).toHaveProperty('message');
    ws.close();
  });

  it('controller receives UNITY_READY when connecting after Unity', async () => {
    const unity = await connectWs('ws://127.0.0.1:' + port);
    await waitForMessage(unity.messages, 'HANDSHAKE');
    await send(unity.ws, { type: 'HANDSHAKE_ACK', client: 'unity' });

    const controller = await connectWs('ws://127.0.0.1:' + port);
    await waitForMessage(controller.messages, 'HANDSHAKE');
    await waitForMessage(controller.messages, 'UNITY_READY');
    expect(controller.messages.find((m) => m.type === 'UNITY_READY').value).toBe(true);

    unity.ws.close();
    controller.ws.close();
  });

  it('controller ADD_SCORE is forwarded to Unity client', async () => {
    const unity = await connectWs('ws://127.0.0.1:' + port);
    await waitForMessage(unity.messages, 'HANDSHAKE');
    await send(unity.ws, { type: 'HANDSHAKE_ACK', client: 'unity' });
    await waitForMessage(unity.messages, 'UNITY_READY');

    const controller = await connectWs('ws://127.0.0.1:' + port);
    await waitForMessage(controller.messages, 'HANDSHAKE');
    await send(controller.ws, { type: 'ADD_SCORE', payload: { amount: 10 } });

    const addScore = await waitForMessage(unity.messages, 'ADD_SCORE');
    expect(addScore.payload).toEqual({ amount: 10 });

    unity.ws.close();
    controller.ws.close();
  });
});
