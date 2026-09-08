const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const script = fs.readFileSync(path.join(root, 'setup/YouTubeBackground.iss'), 'utf8');
const languages = [...script.matchAll(/^Name: "([^"]+)"; MessagesFile: "[^"]*,([^"\r\n]+)"/gm)];

function messages(file) {
  const values = new Map();
  let section = '';
  for (const line of fs.readFileSync(path.join(root, 'setup', file.replaceAll('\\', '/')), 'utf8').split(/\r?\n/)) {
    if (line.startsWith('[')) { section = line; continue; }
    if (!line.trim() || line.startsWith(';')) continue;
    const separator = line.indexOf('=');
    assert.ok(separator > 0, `Invalid message: ${line}`);
    const key = section + line.slice(0, separator);
    assert.ok(!values.has(key), `Duplicate message: ${key}`);
    const value = line.slice(separator + 1);
    assert.ok(value.trim(), `Empty message: ${key}`);
    values.set(key, value);
  }
  return values;
}

test('every installer language translates all messages and preserves argument placeholders', () => {
  assert.equal(languages.length, 3);
  const baseline = messages(languages[0][2]);
  const placeholders = value => [...value.matchAll(/%[1-9]/g)].map(m => m[0]).sort();
  for (const [, language, file] of languages) {
    const translated = messages(file);
    assert.deepEqual([...translated.keys()].sort(), [...baseline.keys()].sort(), language);
    for (const [key, value] of baseline) {
      assert.deepEqual(placeholders(translated.get(key)), placeholders(value), `${language}: ${key}`);
    }
    for (const ref of script.matchAll(/CustomMessage\('([^']+)'\)|\{cm:([^},]+)/g)) {
      assert.ok(translated.has('[CustomMessages]' + (ref[1] || ref[2])), `${language}: missing ${ref[0]}`);
    }
  }
});
