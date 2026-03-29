var __srWsNextId = 1;
var __srWsSockets = {};

mergeInto(LibraryManager.library, {

	SR_WS_Connect: function (urlPtr) {
		var url = UTF8ToString(urlPtr);
		try {
			var ws = new WebSocket(url);
			var id = __srWsNextId++;
			var entry = { ws: ws, queue: [] };
			ws.onmessage = function (e) {
				if (typeof e.data === 'string') {
					entry.queue.push(e.data);
				}
			};
			ws.onerror = function () {
				entry.failed = true;
			};
			ws.onopen = function () {
				entry.open = true;
			};
			ws.onclose = function () {
				entry.closed = true;
			};
			__srWsSockets[id] = entry;
			return id;
		} catch (err) {
			return -1;
		}
	},

	SR_WS_ReadyState: function (id) {
		var entry = __srWsSockets[id];
		if (!entry || !entry.ws) {
			return 3;
		}
		return entry.ws.readyState;
	},

	SR_WS_Dequeue: function (id, bufferPtr, maxLen) {
		var entry = __srWsSockets[id];
		if (!entry || entry.queue.length === 0) {
			return 0;
		}
		var msg = entry.queue.shift();
		var len = lengthBytesUTF8(msg);
		if (len + 1 > maxLen) {
			return -1;
		}
		stringToUTF8(msg, bufferPtr, maxLen);
		return len;
	},

	SR_WS_SendStr: function (id, strPtr) {
		var entry = __srWsSockets[id];
		if (!entry || !entry.ws || entry.ws.readyState !== 1) {
			return 0;
		}
		var str = UTF8ToString(strPtr);
		entry.ws.send(str);
		return 1;
	},

	SR_WS_Close: function (id) {
		var entry = __srWsSockets[id];
		if (!entry) {
			return;
		}
		try {
			if (entry.ws && entry.ws.readyState === 1) {
				entry.ws.close();
			}
		} catch (e) {}
		delete __srWsSockets[id];
	}
});
