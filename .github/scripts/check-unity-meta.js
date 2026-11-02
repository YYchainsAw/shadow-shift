// Check changed Unity assets against the files tracked in the PR merge commit.
const { execFileSync } = require('node:child_process');

const base = process.argv[2];
if (!base) {
  console.error('Usage: node .github/scripts/check-unity-meta.js <base-commit>');
  process.exit(1);
}

function git(...args) {
  return execFileSync('git', args, { maxBuffer: 64 * 1024 * 1024 });
}

function paths(buffer) {
  return buffer.toString('utf8').split('\0').filter(Boolean);
}

const tracked = new Set(paths(git('ls-tree', '-r', '--name-only', '-z', 'HEAD', '--', 'Assets')));
const changed = paths(git('diff', '--name-only', '--no-renames', '-z', base, 'HEAD', '--', 'Assets'));
const errors = new Set();

function isFolderMeta(meta) {
  const contents = git('show', `HEAD:${meta}`).toString('utf8');
  return /^folderAsset:\s*yes\s*$/m.test(contents);
}

for (const path of changed) {
  if (path.endsWith('.meta')) {
    const asset = path.slice(0, -5);
    if (!tracked.has(path) && (tracked.has(asset) || [...tracked].some(p => p.startsWith(`${asset}/`)))) {
      errors.add(`Asset or folder still exists, but its .meta file was deleted: ${path}`);
    }
    if (tracked.has(path) && !tracked.has(asset) && !isFolderMeta(path)) {
      errors.add(`Asset does not exist, but its .meta file is still present: ${path}`);
    }
    continue;
  }

  const meta = `${path}.meta`;
  if (tracked.has(path)) {
    if (!tracked.has(meta)) errors.add(`Missing .meta file: ${meta}`);

    const parts = path.split('/');
    for (let i = 2; i < parts.length; i++) {
      const folderMeta = `${parts.slice(0, i).join('/')}.meta`;
      if (!tracked.has(folderMeta)) errors.add(`Folder is missing its .meta file: ${folderMeta}`);
    }
  } else if (tracked.has(meta) && !isFolderMeta(meta)) {
    errors.add(`Asset was deleted, but its .meta file is still present: ${meta}`);
  }
}

if (errors.size) {
  for (const error of errors) console.error(error);
  process.exit(1);
}
console.log('Unity .meta checks passed.');
