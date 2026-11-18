// Live GitHub feed (pushes + PRs). Geen dependencies, Node 18+.
// Start:  $env:GITHUB_TOKEN="ghp_..." ; node server.js
const http = require("http");
const fs = require("fs");
const path = require("path");

const REPO = process.env.REPO || "15-Braincells-Studios/15-braincells";
const TOKEN = process.env.GITHUB_TOKEN || "";
const PORT = process.env.PORT || 3000;
const INTERVAL = 15000;

const headers = {
  Accept: "application/vnd.github+json",
  "User-Agent": "github-feed",
  ...(TOKEN && { Authorization: `Bearer ${TOKEN}` }),
};
const gh = async (p) => {
  const r = await fetch(`https://api.github.com/repos/${REPO}${p}`, { headers });
  if (!r.ok) throw new Error(`${r.status} ${p}`);
  return r.json();
};

const seenHeads = new Map(); // branch -> sha
let pushes = [];             // nieuwste eerst
let pulls = [];
let status = { ok: false, error: "nog niet gepold", updated: null };

async function poll() {
  try {
    const branches = await gh("/branches?per_page=100");
    for (const b of branches) {
      if (seenHeads.get(b.name) === b.commit.sha) continue;
      const first = !seenHeads.has(b.name);
      seenHeads.set(b.name, b.commit.sha);
      const c = await gh(`/commits/${b.commit.sha}`);
      pushes.unshift({
        branch: b.name,
        sha: c.sha.slice(0, 7),
        message: c.commit.message.split("\n")[0],
        author: c.author?.login || c.commit.author.name,
        avatar: c.author?.avatar_url || null,
        date: c.commit.author.date,
        url: c.html_url,
        isNew: !first,
      });
    }
    pushes.sort((a, b) => new Date(b.date) - new Date(a.date));
    pushes = pushes.slice(0, 50);

    const prs = await gh("/pulls?state=all&sort=updated&direction=desc&per_page=20");
    pulls = prs.map((p) => ({
      number: p.number,
      title: p.title,
      author: p.user.login,
      avatar: p.user.avatar_url,
      state: p.merged_at ? "merged" : p.draft ? "draft" : p.state,
      branch: `${p.head.ref} → ${p.base.ref}`,
      updated: p.updated_at,
      url: p.html_url,
    }));
    status = { ok: true, error: null, updated: new Date().toISOString() };
  } catch (e) {
    status = { ok: false, error: e.message, updated: status.updated };
  }
}
poll();
setInterval(poll, INTERVAL);

http
  .createServer((req, res) => {
    if (req.url.startsWith("/api/feed")) {
      res.writeHead(200, { "Content-Type": "application/json" });
      return res.end(JSON.stringify({ repo: REPO, interval: INTERVAL, status, pushes, pulls }));
    }
    res.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
    res.end(fs.readFileSync(path.join(__dirname, "index.html")));
  })
  .listen(PORT, () => console.log(`Feed op http://localhost:${PORT}  (repo: ${REPO}, token: ${TOKEN ? "ja" : "NEE"})`));
