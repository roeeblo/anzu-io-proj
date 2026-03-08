const path = require('path');
module.exports = {
  testEnvironment: 'node',
  rootDir: path.resolve(__dirname),
  testMatch: ['**/tests/**/*.test.js'],
  testPathIgnorePatterns: ['/node_modules/'],
  testTimeout: 15000,
  verbose: true,
  projects: [
    { displayName: 'node', testMatch: ['**/tests/unit/**/*.test.js', '**/tests/integration/**/*.test.js', '**/tests/db/**/*.test.js'], testEnvironment: 'node' },
    { displayName: 'ui', testMatch: ['**/tests/ui/**/*.test.js'], testEnvironment: 'jsdom' },
  ],
};
