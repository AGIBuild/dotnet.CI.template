import killPort from 'kill-port';
import { spawn } from 'node:child_process';

const args = process.argv.slice(2);
const port = readOption(args, '--port') ?? '5173';
const host = readOption(args, '--host') ?? '0.0.0.0';

await stopPort(port);
await runDocsDevServer(host, port);

function readOption(values, name) {
  const index = values.indexOf(name);
  return index >= 0 && index + 1 < values.length ? values[index + 1] : null;
}

async function stopPort(portValue) {
  try {
    await killPort(portValue, 'tcp');
    console.log(`Stopped existing docs server on port ${portValue}.`);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    if (message.includes('No process running on port')) {
      console.log(`No existing docs server was listening on port ${portValue}.`);
      return;
    }

    throw error;
  }
}

function runDocsDevServer(hostValue, portValue) {
  const command = process.platform === 'win32'
    ? { fileName: 'cmd.exe', args: ['/c', 'npm.cmd', 'run', 'docs:dev', '--', '--host', hostValue, '--port', portValue] }
    : { fileName: 'npm', args: ['run', 'docs:dev', '--', '--host', hostValue, '--port', portValue] };

  return new Promise((resolve, reject) => {
    const childProcess = spawn(command.fileName, command.args, {
      cwd: process.cwd(),
      stdio: 'inherit',
    });

    childProcess.on('close', (code) => {
      if (code === 0 || code === null) resolve();
      else reject(new Error(`Docs dev server exited with code ${code}.`));
    });

    childProcess.on('error', reject);
  });
}