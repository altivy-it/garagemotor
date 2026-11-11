const config = {
  testDir: '.',
  testMatch: '**/*.spec.ts',
  fullyParallel: false,
  forbidOnly: true,
  retries: 0,
  reporter: 'line',
  outputDir: 'test-results',
  timeout: 30_000,
  expect: {
    timeout: 5_000,
  },
  use: {
    baseURL: process.env.GARAGEMOTOR_BASE_URL ?? 'http://nginx:8080',
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
  },
};

export default config;
