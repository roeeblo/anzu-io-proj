function createMockWs(options = {}) {
  const { readyState = 1, sendCalls = [] } = options;
  const sent = [];

  const mockWs = {
    readyState,
    send(data) {
      sent.push(data);
      if (Array.isArray(sendCalls)) sendCalls.push(data);
    },
    getSent() {
      return [...sent];
    },
    getSentParsed() {
      return sent.map((s) => JSON.parse(s));
    },
    reset() {
      sent.length = 0;
    },
  };

  return mockWs;
}

module.exports = { createMockWs };
