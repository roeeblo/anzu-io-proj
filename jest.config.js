/** @type {import('jest').Config} */
const path = require('path');
module.exports = {
  testEnvironment: 'node',
  rootDir: path.resolve(__dirname),
  testMatch: ['**/tests/**/*.test.js'],
  testPathIgnorePatterns: ['/node_modules/'],
  testTimeout: 15000,
  verbose: true,
};
