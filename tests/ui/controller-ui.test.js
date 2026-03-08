/** @jest-environment jsdom */
const { TextEncoder, TextDecoder } = require('util');
if (typeof global.TextEncoder === 'undefined') global.TextEncoder = TextEncoder;
if (typeof global.TextDecoder === 'undefined') global.TextDecoder = TextDecoder;

const fs = require('fs');
const path = require('path');
const { JSDOM } = require('jsdom');

const htmlPath = path.join(__dirname, '../../server/public/index.html');
const html = fs.readFileSync(htmlPath, 'utf8');

function createMockWebSocket(win) {
  const sent = [];
  let instance = null;
  const schedule = win && typeof win.setTimeout === 'function' ? (fn) => win.setTimeout(fn, 0) : (fn) => setImmediate(fn);

  class MockWebSocket {
    static get sent() {
      return sent;
    }

    constructor(url) {
      this.url = url;
      this.readyState = 0;
      this.onopen = null;
      this.onclose = null;
      this.onerror = null;
      this.onmessage = null;
      instance = this;
      schedule(() => {
        this.readyState = 1;
        if (this.onopen) this.onopen();
      });
    }

    send(data) {
      sent.push(data);
    }

    close() {}
  }

  MockWebSocket.OPEN = 1;
  MockWebSocket.instance = () => instance;
  MockWebSocket.clearSent = () => sent.length = 0;
  return MockWebSocket;
}

describe('Controller UI', () => {
  let dom;
  let window;
  let document;
  let MockWs;

  beforeAll(() => {
    console.log('[Controller UI] suite running');
    dom = new JSDOM(html, {
      runScripts: 'dangerously',
      url: 'http://localhost:8080',
      beforeParse(win) {
        win.setImmediate = win.setImmediate || (typeof setImmediate !== 'undefined' ? setImmediate : (fn) => win.setTimeout(fn, 0));
        MockWs = createMockWebSocket(win);
        win.WebSocket = MockWs;
      },
    });
    window = dom.window;
    document = window.document;
  });

  beforeEach(() => {
    MockWs.clearSent();
  });

  it('shows Connecting then Connected after WebSocket open', (done) => {
    const status = document.getElementById('status');
    expect(status).not.toBeNull();
    expect(status.textContent).toMatch(/Connecting|Connected/);
    window.setTimeout(() => {
      expect(status.textContent).toBe('Connected to server');
      expect(status.className).toContain('connected');
      done();
    }, 100);
  }, 500);

  it('shows Unity ready when UNITY_READY message received', (done) => {
    const unityStatus = document.getElementById('unityStatus');
    expect(unityStatus).not.toBeNull();
    window.setTimeout(() => {
      const ws = MockWs.instance();
      if (ws && ws.onmessage) {
        ws.onmessage({ data: JSON.stringify({ type: 'UNITY_READY', value: true }) });
        expect(unityStatus.textContent).toMatch(/Unity ready/i);
        expect(unityStatus.className).toContain('connected');
      }
      done();
    }, 120);
  }, 500);

  it('sends MOVE_LEFT when MOVE LEFT button clicked', (done) => {
    window.setTimeout(() => {
      const btn = document.querySelector('[data-cmd="MOVE_LEFT"]');
      expect(btn).not.toBeNull();
      btn.click();
      expect(MockWs.sent.length).toBeGreaterThanOrEqual(1);
      const last = JSON.parse(MockWs.sent[MockWs.sent.length - 1]);
      expect(last.type).toBe('MOVE_LEFT');
      done();
    }, 120);
  }, 500);

  it('sends ADD_SCORE with amount from input when Add Score clicked', (done) => {
    window.setTimeout(() => {
      const amountInput = document.getElementById('scoreAmount');
      const btn = document.getElementById('btnAddScore');
      amountInput.value = '50';
      btn.click();
      expect(MockWs.sent.length).toBeGreaterThanOrEqual(1);
      const last = JSON.parse(MockWs.sent[MockWs.sent.length - 1]);
      expect(last.type).toBe('ADD_SCORE');
      expect(last.payload).toEqual({ amount: 50 });
      done();
    }, 120);
  }, 500);

  it('sends SPAWN_ITEM with itemId and x,y when Spawn clicked', (done) => {
    window.setTimeout(() => {
      document.getElementById('itemId').value = 'cherry';
      document.getElementById('spawnX').value = '3';
      document.getElementById('spawnY').value = '-2';
      document.getElementById('btnSpawn').click();
      expect(MockWs.sent.length).toBeGreaterThanOrEqual(1);
      const last = JSON.parse(MockWs.sent[MockWs.sent.length - 1]);
      expect(last.type).toBe('SPAWN_ITEM');
      expect(last.payload.prefabType).toBe('cherry');
      expect(last.payload.x).toBe(3);
      expect(last.payload.y).toBe(-2);
      done();
    }, 120);
  }, 500);

  it('shows Game Over and disables commands when GAME_OVER message received', (done) => {
    window.setTimeout(() => {
      const gameOverStatus = document.getElementById('gameOverStatus');
      const commandArea = document.getElementById('commandArea');
      const ws = MockWs.instance();
      if (ws && ws.onmessage) {
        ws.onmessage({ data: JSON.stringify({ type: 'GAME_OVER' }) });
        expect(gameOverStatus.style.display).not.toBe('none');
        expect(gameOverStatus.textContent).toMatch(/Game Over/i);
        expect(commandArea.classList.contains('disabled')).toBe(true);
      }
      done();
    }, 120);
  }, 500);
});
