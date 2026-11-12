const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { spawn } = require('node:child_process');
const { isIP, createServer, connect } = require('node:net');

const caPath = '/trust/rootCA.pem';
if (isIP(process.env.GARAGEMOTOR_GATEWAY_IP || '') !== 4) throw new Error('Public gateway IPv4 must be provided to the isolated runner.');
if (!fs.existsSync(caPath)) throw new Error('Public Identity CA must be mounted in the runner.');
const runtime = fs.mkdtempSync(path.join(os.tmpdir(), 'garagemotor-firefox-'));
const policies = path.join(runtime, 'policies.json');
fs.writeFileSync(policies, JSON.stringify({
  policies: {
    Certificates: { Install: [caPath] },
    DisableSecurityBypass: { InvalidCertificate: true },
    DisableAppUpdate: true,
    DisableTelemetry: true,
  },
}), { flag: 'wx', mode: 0o600 });
const environment = { ...process.env, PLAYWRIGHT_FIREFOX_POLICIES_JSON: policies, NODE_EXTRA_CA_CERTS: caPath, GARAGEMOTOR_SHARED_TESTS: '1' };
delete environment.DEBUG;
delete environment.PWDEBUG;
delete environment.NODE_DEBUG;
const cli = path.resolve(__dirname, '../../web/node_modules/@playwright/test/cli.js');
const bridge = createServer((client) => {
  const upstream = connect(7443, process.env.GARAGEMOTOR_GATEWAY_IP);
  client.on('error', () => upstream.destroy());
  upstream.on('error', () => client.destroy());
  client.pipe(upstream).pipe(client);
});
bridge.listen(7443, '0.0.0.0', () => {
  const child = spawn(process.execPath, [cli, 'test', '--config', path.resolve(__dirname, '../shared.playwright.config.ts'), '--output', path.join(runtime, 'test-results'), ...process.argv.slice(2)], {
    cwd: path.resolve(__dirname, '..'),
    env: environment,
    stdio: 'inherit',
  });
  child.on('exit', (status) => {
    bridge.close();
    fs.rmSync(runtime, { recursive: true, force: true });
    process.exit(status ?? 1);
  });
});
