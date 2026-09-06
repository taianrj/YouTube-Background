// Real Chrome, isolated disposable profile, deterministic media fixture.
// Does not install a native host or touch the user's existing Chrome profile.
const { spawn } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const chrome = process.env.CHROME_PATH || 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const profile = path.resolve(process.argv[2] || 'browser-test-profile');
fs.mkdirSync(profile, { recursive: true });
const proc = spawn(chrome, ['--headless=new','--no-first-run','--no-default-browser-check','--autoplay-policy=no-user-gesture-required','--remote-debugging-pipe','--enable-unsafe-extension-debugging',`--user-data-dir=${profile}`], { windowsHide: true, stdio: ['ignore','ignore','pipe','pipe','pipe'] });
let serial = 0, buffer = '', events = [];
const pending = new Map();
let stderr = '';
proc.stderr.on('data', b => { stderr = (stderr + b).slice(-4000); });
proc.stdio[4].on('data', chunk => {
  buffer += chunk.toString();
  let end;
  while ((end = buffer.indexOf('\0')) >= 0) {
    const raw = buffer.slice(0, end); buffer = buffer.slice(end + 1);
    if (!raw) continue;
    const m = JSON.parse(raw);
    if (m.id && pending.has(m.id)) { const p = pending.get(m.id); pending.delete(m.id); clearTimeout(p.timeout); m.error ? p.reject(new Error(JSON.stringify(m.error))) : p.resolve(m.result); }
    else for (const fn of events) fn(m);
  }
});
function call(method, params = {}, sessionId) {
  return new Promise((resolve, reject) => {
    const id = ++serial;
    const timeout = setTimeout(() => { pending.delete(id); reject(new Error('Timeout: ' + method + '\n' + stderr)); }, 10000);
    pending.set(id, { resolve, reject, timeout });
    proc.stdio[3].write(JSON.stringify({ id, method, params, ...(sessionId ? { sessionId } : {}) }) + '\0');
  });
}
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function evalJS(session, expression) {
  const r = await call('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true }, session);
  if (r.exceptionDetails) throw new Error(JSON.stringify(r.exceptionDetails));
  return r.result.value;
}
function wav() {
  const data = Buffer.alloc(44 + 8000 * 60 * 2);
  data.write('RIFF'); data.writeUInt32LE(data.length - 8, 4); data.write('WAVEfmt ', 8); data.writeUInt32LE(16,16);
  data.writeUInt16LE(1,20); data.writeUInt16LE(1,22); data.writeUInt32LE(8000,24); data.writeUInt32LE(16000,28); data.writeUInt16LE(2,32); data.writeUInt16LE(16,34); data.write('data',36); data.writeUInt32LE(data.length-44,40);
  return data.toString('base64');
}
(async () => {
  try {
    const version = await call('Browser.getVersion');
    const ext = await call('Extensions.loadUnpacked', { path: path.resolve(__dirname, '../extension') });
    assert.equal(ext.id, 'jobnoaknnkhgmfjfbfkelbpfgabdogfb');
    const html = `<title>Fixture YouTube</title><div id="movie_player"><video muted autoplay src="data:audio/wav;base64,${wav()}"></video></div>`;
    const tab = await call('Target.createTarget', { url: 'about:blank' });
    const { sessionId } = await call('Target.attachToTarget', { targetId: tab.targetId, flatten: true });
    events.push(m => {
      if (m.method === 'Fetch.requestPaused' && m.sessionId === sessionId) call('Fetch.fulfillRequest', { requestId: m.params.requestId, responseCode: 200, responseHeaders: [{ name: 'Content-Type', value: 'text/html' }], body: Buffer.from(html).toString('base64') }, sessionId).catch(() => {});
    });
    await call('Fetch.enable', { patterns: [{ urlPattern: 'https://www.youtube.com/*', resourceType: 'Document' }] }, sessionId);
    await call('Page.navigate', { url: 'https://www.youtube.com/watch?v=local-test-fixture' }, sessionId);
    let ready = false;
    for (let i = 0; i < 40 && !ready; i++) { await sleep(100); ready = await evalJS(sessionId, '!!document.querySelector("video") && document.querySelector("video").readyState >= 2'); }
    assert.ok(ready, 'fixture media loaded');
    await evalJS(sessionId, 'document.querySelector("video").currentTime = 20; document.querySelector("video").muted = false; document.querySelector("video").play()');
    let worker;
    for (let i = 0; i < 30 && !worker; i++) { worker = (await call('Target.getTargets')).targetInfos.find(t => t.type === 'service_worker' && t.url.startsWith(`chrome-extension://${ext.id}/`)); if (!worker) await sleep(100); }
    assert.ok(worker, 'extension service worker running');
    const attached = await call('Target.attachToTarget', { targetId: worker.targetId, flatten: true });
    const ws = attached.sessionId;
    await sleep(250);
    const state = await evalJS(ws, '(async()=>{await refresh(); return YTCore.selectTarget([...videos.values()]);})()');
    assert.ok(state?.playing, 'content script reports playing video');
    await call('Target.createTarget', { url: 'about:blank' });
    const send = delta => evalJS(ws, `chrome.tabs.sendMessage(${state.tabId}, {v:1,type:'seek',id:'test',tabId:${state.tabId},delta:${delta},expiresAt:Date.now()+1500})`);
    let before = await evalJS(sessionId, 'document.querySelector("video").currentTime');
    const firstSeek = await send(5);
    assert.equal(firstSeek.ok, true, JSON.stringify({ firstSeek, video: await evalJS(sessionId, '(()=>{let v=document.querySelector("video");return {paused:v.paused,ended:v.ended,ready:v.readyState,time:v.currentTime,duration:v.duration};})()') }));
    let after = await evalJS(sessionId, 'document.querySelector("video").currentTime');
    assert.ok(after - before >= 4.9 && after - before < 6);
    before = after;
    assert.equal((await send(-5)).ok, true);
    after = await evalJS(sessionId, 'document.querySelector("video").currentTime');
    assert.ok(before - after >= 4 && before - after <= 5.1);
    await evalJS(sessionId, 'document.querySelector("video").pause()');
    assert.equal((await send(5)).ok, false);
    await evalJS(sessionId, 'document.querySelector("video").play(); document.querySelector("#movie_player").classList.add("ad-showing")');
    assert.equal((await send(5)).ok, false);
    await call('Target.closeTarget', { targetId: tab.targetId });
    await sleep(150);
    const gone = await evalJS(ws, '(async()=>{await refresh(); return YTCore.selectTarget([...videos.values()]);})()');
    assert.equal(gone, null);
    console.log('PASS: ' + version.product + '; extension loaded, real media seek from another tab, pause/ad rejection, closed-tab removal. Synthetic YouTube fixture; no live-site assertion.');
    if (process.argv.includes('--live')) {
      const live = await call('Target.createTarget', { url: 'about:blank' });
      const liveSession = (await call('Target.attachToTarget', { targetId: live.targetId, flatten: true })).sessionId;
      await call('Page.navigate', { url: 'https://www.youtube.com/watch?v=aqz-KE-bpKQ' }, liveSession);
      let liveState;
      for (let i = 0; i < 50; i++) {
        await sleep(400);
        liveState = await evalJS(liveSession, '(()=>{const v=document.querySelector("#movie_player video");return {title:document.title,url:location.href,media:!!v,ready:v?.readyState,ad:!!document.querySelector("#movie_player.ad-showing"),message:document.querySelector(".ytp-error-content-wrap")?.innerText};})()');
        if (liveState.ready >= 2 || liveState.message) break;
      }
      console.log('LIVE_SITE: ' + JSON.stringify(liveState));
      if (!liveState?.media || liveState.ready < 2 || liveState.ad) throw new Error('Live YouTube playback unavailable; manual validation remains required.');
      await evalJS(liveSession, 'document.querySelector("video").play()');
      await sleep(400);
      const target = await evalJS(ws, '(async()=>{await refresh(); return YTCore.selectTarget([...videos.values()]);})()');
      assert.ok(target?.playing);
      await call('Target.createTarget', { url: 'about:blank' });
      const previous = await evalJS(liveSession, 'document.querySelector("video").currentTime');
      if (process.argv.includes('--native')) {
        await sleep(400);
        require('node:child_process').execFileSync('dotnet', [path.resolve(__dirname, 'Integration/bin/Debug/net10.0-windows/Integration.dll'), '--trigger', '2'], { windowsHide: true });
        await sleep(400);
      } else {
        const result = await evalJS(ws, `chrome.tabs.sendMessage(${target.tabId},{v:1,type:'seek',id:'live',tabId:${target.tabId},delta:5,expiresAt:Date.now()+1500})`);
        assert.ok(result.ok, JSON.stringify(result));
      }
      const current = await evalJS(liveSession, 'document.querySelector("video").currentTime');
      assert.ok(current - previous >= 4.5 && current - previous < 7);
      console.log('PASS: live YouTube video advanced from another tab' + (process.argv.includes('--native') ? ' via Windows app + native host + extension.' : '.'));
    }
  } finally {
    try { await call('Browser.close'); } catch {}
    if (proc.exitCode === null) proc.kill();
    for (const p of pending.values()) clearTimeout(p.timeout);
  }
})().catch(e => { console.error(e); process.exitCode = 1; });
