/** @type {import('jest').Config} */
module.exports = {
  testEnvironment: 'node',
  rootDir: '..',
  testMatch: ['tests/unit/**/*.test.js'],
  testPathIgnorePatterns: ['/node_modules/'],
  verbose: true,
};
