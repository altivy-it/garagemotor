const config = {
  testDir: '.',
  testMatch: ['integrated/**/*.spec.ts', 'contracts/openapi.spec.ts', '**/design-system/theme.spec.ts'],
  fullyParallel: false,
  workers: 1,
  forbidOnly: true,
  retries: 0,
  reporter: 'line',
  outputDir: 'test-results-shared',
  timeout: 45_000,
  expect: { timeout: 10_000 },
  use: {
    baseURL: 'https://localhost:7443/garagemotor/',
    browserName: 'firefox',
    ignoreHTTPSErrors: false,
    screenshot: 'off',
    trace: 'off',
    video: 'off',
  },
};

export default config;
