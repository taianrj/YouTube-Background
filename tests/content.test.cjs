const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');
function page() {
  const listeners = {}, reports = [];
  const v = { paused: false, ended: false, readyState: 4, currentTime: 50, duration: 100,
    seekable: { length: 1, start: () => 0, end: () => 100 } };
  const state = { ad: false, video: v };
  let handler;
  const context = vm.createContext({ Date, console, setTimeout,
    document: { title: 'Teste YouTube', documentElement: {},
      querySelector: q => q === '#movie_player video' ? state.video : state.ad ? {} : null,
      addEventListener: (name, fn) => { listeners[name] = fn; } },
    MutationObserver: class { observe() {} },
    chrome: { runtime: { sendMessage: async m => { reports.push(m); }, onMessage: { addListener: fn => { handler = fn; } } } }
  });
  for (const file of ['core.js','content.js']) vm.runInContext(fs.readFileSync(path.join(__dirname,'../extension',file),'utf8'), context);
  const send = (overrides = {}) => { let result; handler({ v: 1, type: 'seek', id: 'test', tabId: 1, delta: 5, expiresAt: Date.now() + 1000, ...overrides }, {}, r => { result = r; }); return result; };
  return { v, state, send, listeners, reports };
}
test('salta sem pausar ou depender de foco', () => {
  const p = page(); assert.equal(p.send().ok, true); assert.equal(p.v.currentTime, 55); assert.equal(p.v.paused, false);
});
test('revalida pausa, anúncio e expiração imediatamente antes do salto', () => {
  const p = page(); p.v.paused = true;
  assert.equal(p.send().ok, false); p.v.paused = false; p.state.ad = true;
  assert.equal(p.send().ok, false); p.state.ad = false;
  assert.equal(p.send({ expiresAt: Date.now() - 100 }).ok, false); assert.equal(p.v.currentTime, 50);
});
test('trata navegação, substituição do elemento e aba sem vídeo', () => {
  const p = page(); p.state.video = null; p.listeners['yt-navigate-finish']();
  assert.equal(p.send().ok, false); assert.equal(p.reports.at(-1).playing, false);
  p.state.video = { ...p.v, currentTime: 10 }; p.listeners.loadedmetadata();
  assert.equal(p.send({ delta: -5 }).ok, true); assert.equal(p.state.video.currentTime, 5);
});
test('transmissão sem DVR não é alterada', () => {
  const p = page(); p.v.duration = Infinity; p.v.seekable.length = 0;
  assert.equal(p.send().ok, false); assert.equal(p.v.currentTime, 50);
});
test('repetição de eventos sem mudança não gera mensagens adicionais', () => {
  const p = page(); const before = p.reports.length;
  p.listeners.playing(); p.listeners.playing(); assert.equal(p.reports.length, before);
});
