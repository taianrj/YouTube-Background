importScripts('core.js');
let port = null, reconnectTimer = null, revision = 0;
const videos = new Map();
function publish() {
  if (!port) return;
  const selected = YTCore.selectTarget([...videos.values()]);
  try { port.postMessage({ v: 1, type: 'state', target: selected }); } catch { /* disconnect event reconnects */ }
}
async function refresh() {
  const observedRevision = revision;
  const tabs = await chrome.tabs.query({ url: 'https://www.youtube.com/*' });
  const found = new Map();
  await Promise.all(tabs.map(async tab => {
    try {
      const s = await chrome.tabs.sendMessage(tab.id, { type: 'probe' });
      const item = { ...s, tabId: tab.id };
      if (YTCore.validVideo(item)) found.set(tab.id, item);
    } catch { /* unloaded, discarded or pre-install page */ }
  }));
  if (revision === observedRevision) { videos.clear(); for (const [id, s] of found) videos.set(id, s); }
  publish();
}
function connect() {
  if (port) return;
  port = chrome.runtime.connectNative('com.youtubebackground.host');
  port.onDisconnect.addListener(() => {
    void chrome.runtime.lastError;
    port = null;
    clearTimeout(reconnectTimer); reconnectTimer = setTimeout(connect, 5000);
  });
  port.onMessage.addListener(async m => {
    if (!YTCore.validSeek(m)) return;
    const connection = port;
    await refresh();
    const target = YTCore.selectTarget([...videos.values()]);
    let result = { ok: false, reason: 'O vídeo selecionado mudou ou parou.' };
    if (port === connection && target?.tabId === m.tabId && YTCore.validSeek(m)) {
      try { result = await chrome.tabs.sendMessage(m.tabId, m); }
      catch { result = { ok: false, reason: 'A aba não está disponível.' }; }
    }
    if (port === connection) { try { port.postMessage({ v: 1, type: 'result', id: m.id, ...result }); } catch {} }
  });
  refresh().catch(() => {});
}
chrome.runtime.onMessage.addListener((m, sender) => {
  if (!sender.tab || sender.frameId !== 0 || !sender.url?.startsWith('https://www.youtube.com/') || m.type !== 'video-state') return;
  const item = { tabId: sender.tab.id, playing: m.playing, ad: m.ad, startedAt: m.startedAt, title: m.title };
  if (YTCore.validVideo(item)) { revision++; videos.set(item.tabId, item); publish(); }
});
chrome.tabs.onRemoved.addListener(id => { revision++; videos.delete(id); publish(); });
chrome.tabs.onUpdated.addListener((id, info) => { if (info.status === 'loading') { revision++; videos.delete(id); publish(); } });
chrome.alarms.create('reconnect', { periodInMinutes: 0.5 });
chrome.alarms.onAlarm.addListener(() => { connect(); refresh().catch(() => {}); });
connect();
