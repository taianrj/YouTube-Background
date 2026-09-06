/* Shared pure logic, also exercised by the Node tests. */
(function (root) {
  const validVideo = x => x && Number.isInteger(x.tabId) && x.tabId >= 0 &&
    typeof x.playing === 'boolean' && typeof x.ad === 'boolean' &&
    Number.isFinite(x.startedAt) && x.startedAt >= 0 && typeof x.title === 'string' && x.title.length <= 300;
  function selectTarget(videos) {
    return videos.filter(x => validVideo(x) && x.playing && !x.ad)
      .sort((a, b) => b.startedAt - a.startedAt || a.tabId - b.tabId)[0] || null;
  }
  function seekPosition(current, delta, ranges) {
    if (!Number.isFinite(current) || !Number.isFinite(delta) || !ranges.length) return null;
    const desired = current + delta;
    const points = ranges.filter(([a, b]) => Number.isFinite(a) && Number.isFinite(b) && b >= a);
    if (!points.length) return null;
    for (const [a, b] of points) if (desired >= a && desired <= b) return desired;
    return points.flat().reduce((best, n) => Math.abs(n - desired) < Math.abs(best - desired) ? n : best);
  }
  function validSeek(m, now = Date.now()) {
    return !!m && m.v === 1 && m.type === 'seek' && typeof m.id === 'string' && m.id.length > 0 && m.id.length <= 64 &&
      Number.isInteger(m.tabId) && m.tabId >= 0 && Number.isInteger(m.delta) &&
      Math.abs(m.delta) >= 1 && Math.abs(m.delta) <= 120 &&
      Number.isFinite(m.expiresAt) && m.expiresAt >= now && m.expiresAt <= now + 3000;
  }
  root.YTCore = { validVideo, selectTarget, seekPosition, validSeek };
  if (typeof module !== 'undefined') module.exports = root.YTCore;
})(globalThis);
