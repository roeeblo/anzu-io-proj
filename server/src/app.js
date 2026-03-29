const http = require('http');
const fs = require('fs');
const path = require('path');
const WebSocket = require('ws');
const readline = require('readline');
const { create: createCommand, getValidTypes } = require('./commands/CommandFactory');
const { connect: connectDb, getDb } = require('./db');

const COMMAND_TYPES = new Set(['MOVE_LEFT', 'MOVE_RIGHT', 'STOP', 'JUMP', 'SPAWN_ITEM', 'ADD_SCORE', 'TREE1_COMMAND', 'TREE2_COMMAND']);
let currentClient = null;
const allConnections = new Set();
let db = null;

const BOUNDS = {
  minX: Number(process.env.BOUNDS_MIN_X) || -20,
  maxX: Number(process.env.BOUNDS_MAX_X) || 20,
  minY: Number(process.env.BOUNDS_MIN_Y) || -15,
  maxY: Number(process.env.BOUNDS_MAX_Y) || 15,
};

function isUnityClient(ws) {
  return ws === currentClient;
}

function isSocketOpen(ws) {
  return ws && ws.readyState === WebSocket.OPEN;
}

function isWithinBounds(x, y, bounds) {
  return x >= bounds.minX && x <= bounds.maxX && y >= bounds.minY && y <= bounds.maxY;
}

function logStatus(msg) {
  process.stderr.write(msg + '\n');
}

function broadcastToControllers(obj) {
  const msg = JSON.stringify(obj);
  allConnections.forEach((ws) => {
    if (ws !== currentClient && ws.readyState === WebSocket.OPEN) ws.send(msg);
  });
}

function handleHandshakeAck(ws) {
  currentClient = ws;
  logStatus('Unity Client Connected!');
  console.log('Handshake Complete!');
  broadcastToControllers({ type: 'UNITY_READY', value: true });
}

function handleUnityMessage(data) {
  console.log('Received from Unity:', data);
  if (data.type === 'COMMAND_DONE') {
    console.log('Command done:', data.payload?.command);
  }
  if (data.type === 'GAME_OVER') {
    broadcastToControllers(data);
  }
}

function handleControllerMessage(ws, data) {
  if (!isSocketOpen(currentClient)) return;
  if (!COMMAND_TYPES.has(data.type)) return;

  if (data.type === 'SPAWN_ITEM' && data.payload != null) {
    const x = Number(data.payload.x);
    const y = Number(data.payload.y);
    if (Number.isNaN(x) || Number.isNaN(y) || !isWithinBounds(x, y, BOUNDS)) {
      const errMsg =
        'SPAWN_ITEM out of bounds. X: ' + BOUNDS.minX + '-' + BOUNDS.maxX +
        ', Y: ' + BOUNDS.minY + '-' + BOUNDS.maxY +
        '. Got x=' + x + ', y=' + y;
      console.error('[EXCEPTION] ' + errMsg);
      ws.send(JSON.stringify({ type: 'SPAWN_ERROR', message: errMsg, payload: data.payload }));
      return;
    }
  }

  currentClient.send(JSON.stringify(data));
  console.log('Sent (from UI):', data.type, data.payload ? JSON.stringify(data.payload) : '');
}

function sendHandshake(ws) {
  ws.send(JSON.stringify({
    type: 'HANDSHAKE',
    payload: { message: 'Welcome to the Server', serverTime: Date.now() },
  }));
  if (isSocketOpen(currentClient)) {
    ws.send(JSON.stringify({ type: 'UNITY_READY', value: true }));
  }
}

function handleClose(ws) {
  allConnections.delete(ws);
  if (currentClient === ws) {
    currentClient = null;
    logStatus('Client disconnected');
    broadcastToControllers({ type: 'UNITY_READY', value: false });
  }
}

const RECONNECT_WINDOW_MS = 15000;
const RECONNECT_THRESHOLD = 12;
const _connectionTimes = [];

// Close connection if no message received for this many ms. 0 = disabled. Set IDLE_TIMEOUT_MS (e.g. 300000) in production to avoid connections staying open indefinitely.
let idleTimeoutMs = Number(process.env.IDLE_TIMEOUT_MS) || 0;

function trackConnection() {
  const now = Date.now();
  _connectionTimes.push(now);
  while (_connectionTimes.length > 0 && now - _connectionTimes[0] > RECONNECT_WINDOW_MS)
    _connectionTimes.shift();
  if (_connectionTimes.length >= RECONNECT_THRESHOLD) {
    logStatus('[WARNING] Many connect/disconnect in short time. Check client or network.');
  }
}

const server = http.createServer((req, res) => {
  if (req.url === '/' || req.url === '/index.html') {
    const file = path.join(__dirname, '..', 'public', 'index.html');
    fs.readFile(file, (err, data) => {
      if (err) {
        res.writeHead(500);
        res.end('Error loading UI');
        return;
      }
      res.writeHead(200, { 'Content-Type': 'text/html' });
      res.end(data);
    });
  } else {
    res.writeHead(404);
    res.end();
  }
});

const wss = new WebSocket.Server({ server });
const validTypes = getValidTypes();

function start(port = 8080, options = {}) {
  idleTimeoutMs = options.idleTimeoutMs ?? idleTimeoutMs ?? 0;
  return (async () => {
    try {
      db = await connectDb();
      if (process.env.NODE_ENV !== 'test') console.log('[DB] Connected');
    } catch (e) {
      if (process.env.NODE_ENV !== 'test') console.warn('[DB] MongoDB not available:', e.message, '- SPAWN_ITEM disabled');
    }
    return new Promise((resolve, reject) => {
      server.listen(port, () => {
        if (process.env.NODE_ENV !== 'test') {
          console.log('Server: http://localhost:' + server.address().port + '  (open in browser for button UI)');
          console.log('Commands:', validTypes.join(', '));
        }
        resolve(server);
      });
      server.on('error', reject);
    });
  })();
}

if (require.main === module) start(8080);

function scheduleIdleClose(ws) {
  if (!idleTimeoutMs || idleTimeoutMs <= 0) return;
  if (ws._idleTimer) clearTimeout(ws._idleTimer);
  ws._idleTimer = setTimeout(() => {
    ws._idleTimer = null;
    try {
      if (ws.readyState === WebSocket.OPEN) ws.close();
    } catch (e) {}
  }, idleTimeoutMs);
}

function clearIdleTimer(ws) {
  if (ws._idleTimer) {
    clearTimeout(ws._idleTimer);
    ws._idleTimer = null;
  }
}

wss.on('connection', (ws) => {
  trackConnection();
  allConnections.add(ws);
  sendHandshake(ws);
  scheduleIdleClose(ws);

  ws.on('message', (message) => {
    scheduleIdleClose(ws);
    let data;
    try {
      data = JSON.parse(message);
    } catch (e) {
      return;
    }

    if (data.type === 'HANDSHAKE_ACK' && (data.client === 'unity' || data.client == null)) {
      handleHandshakeAck(ws);
      return;
    }

    if (isUnityClient(ws)) {
      handleUnityMessage(data);
      return;
    }

    handleControllerMessage(ws, data);
  });

  ws.on('close', () => {
    clearIdleTimer(ws);
    handleClose(ws);
  });
});

if (require.main === module) {
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  rl.on('line', async (line) => {
  const parts = line.trim().split(/\s+/).filter(Boolean);
  let cmd = (parts[0] || '').toUpperCase();
  let arg = parts.slice(1).join(' ').trim();

  if (cmd === 'SPAWN' && (parts[1] || '').toUpperCase() === 'ITEM') {
    cmd = 'SPAWN_ITEM';
    arg = parts.slice(2).join(' ').trim();
  }
  if (cmd === 'ADD_SCORE') {
    arg = parts.slice(1).join(' ').trim() || '100';
  }

  if (!cmd) return;

  const command = createCommand(cmd, arg, db || getDb(), BOUNDS);
  if (!command) {
    console.log('Unknown command. Use: MOVE_LEFT, TREE1 / 2 _COMMAND, MOVE_RIGHT, STOP, JUMP, SPAWN_ITEM itemId x y, or ADD_SCORE amount');
    return;
  }

  if (isSocketOpen(currentClient)) {
    const sent = await command.execute(currentClient);
    if (sent !== false) {
      console.log('Sent:', command.type, arg ? arg : '');
    }
  } else {
    console.log('No client connected. Start Unity and press Play.');
  }
  });
}

function setIdleTimeoutMs(ms) {
  idleTimeoutMs = ms;
}

module.exports = { start, setIdleTimeoutMs };
