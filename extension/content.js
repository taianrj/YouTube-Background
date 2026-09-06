(() => {
  let video = null, startedAt = 0, wasPlaying = false, lastState = '', reportPending = false;
  const playing = v => !!v && !v.paused && !v.ended && v.readyState >= 2;
  const isAd = () => !!document.querySelector('#movie_player.ad-showing, #movie_player.ad-interrupting');
  function state() {
    const next = document.querySelector('#movie_player video');
    if (next !== video) { video = next; wasPlaying = false; startedAt = 0; }
    const active = playing(video);
    if (active && !wasPlaying) startedAt = Date.now();
    wasPlaying = active;
    return { playing: active, ad: isAd(), startedAt, title: document.title.slice(0, 300) };
  }
  function report() {
    const s = state(), serialized = JSON.stringify(s);
    if (serialized === lastState) return;
    lastState = serialized;
    try { chrome.runtime.sendMessage({ type: 'video-state', ...s }).catch(() => {}); } catch {}
  }
  function scheduleReport() {
    if (reportPending) return;
    reportPending = true;
    setTimeout(() => { reportPending = false; report(); }, 200);
  }
  for (const name of ['playing', 'pause', 'ended', 'emptied', 'loadedmetadata']) document.addEventListener(name, report, true);
  document.addEventListener('yt-navigate-finish', () => { wasPlaying = false; report(); });
  new MutationObserver(scheduleReport).observe(document.documentElement, { subtree: true, attributes: true, attributeFilter: ['class'], childList: true });
  chrome.runtime.onMessage.addListener((m, sender, reply) => {
    if (m.type === 'probe') { reply(state()); return; }
    if (m.type !== 'seek') return;
    const s = state();
    if (!YTCore.validSeek(m) || !s.playing || s.ad) { reply({ ok: false, reason: 'Vídeo indisponível, pausado ou anúncio.' }); return; }
    const ranges = Array.from({ length: video.seekable.length }, (_, i) => [video.seekable.start(i), video.seekable.end(i)]);
    if (!ranges.length && Number.isFinite(video.duration)) ranges.push([0, video.duration]);
    const position = YTCore.seekPosition(video.currentTime, m.delta, ranges);
    if (position === null) { reply({ ok: false, reason: 'Vídeo sem intervalo navegável.' }); return; }
    try { video.currentTime = position; reply({ ok: true }); } catch { reply({ ok: false, reason: 'Não foi possível navegar no vídeo.' }); }
  });
  report();
})();
