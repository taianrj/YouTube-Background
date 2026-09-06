const { test } = require('node:test');
const assert = require('node:assert/strict');
const { selectTarget, seekPosition, validSeek } = require('../extension/core.js');
const video = (tabId, startedAt, extra = {}) => ({ tabId, startedAt, title: 'Vídeo', playing: true, ad: false, ...extra });
test('seleciona o último iniciado entre vídeos em reprodução, inclusive silenciado', () => {
  assert.equal(selectTarget([video(1, 20), video(2, 30, { muted: true }), video(3, 40, { playing: false })]).tabId, 2);
});
test('não seleciona anúncios, pausados ou registros inválidos', () => {
  assert.equal(selectTarget([video(1, 20, { ad: true }), video(2, 30, { playing: false }), { tabId: 3 }]), null);
  assert.equal(selectTarget([]), null);
});
test('resolve empate de forma determinística', () => assert.equal(selectTarget([video(3, 10), video(1, 10)]).tabId, 1));
test('limita saltos ao início e fim do vídeo', () => {
  assert.equal(seekPosition(2, -5, [[0, 100]]), 0);
  assert.equal(seekPosition(98, 5, [[0, 100]]), 100);
  assert.equal(seekPosition(50, -5, [[0, 100]]), 45);
});
test('respeita janela DVR ao vivo e lacunas', () => {
  assert.equal(seekPosition(105, -20, [[100, 200]]), 100);
  assert.equal(seekPosition(195, 20, [[100, 200]]), 200);
  assert.equal(seekPosition(9, 5, [[0, 10], [20, 30]]), 10);
  assert.equal(seekPosition(9, 10, [[0, 10], [20, 30]]), 20);
  assert.equal(seekPosition(5, 5, []), null);
  assert.equal(seekPosition(NaN, 5, [[0, 10]]), null);
});
test('aceita apenas comandos válidos e recentes', () => {
  const m = { v: 1, type: 'seek', id: 'a', tabId: 1, delta: -5, expiresAt: 2000 };
  assert.equal(validSeek(m, 1000), true);
  for (const change of [{ v: 2 }, { delta: 0 }, { delta: 121 }, { delta: 1.5 }, { tabId: -1 }, { expiresAt: 999 }, { expiresAt: 5000 }, { type: 'execute' }]) assert.equal(validSeek({ ...m, ...change }, 1000), false);
});
