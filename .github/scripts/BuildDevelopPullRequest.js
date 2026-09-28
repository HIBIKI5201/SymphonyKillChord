// feature/**/master → develop の Draft PR を、master に取り込まれた PR の本文から組み立てる。
// AutoCreateDevelopPullRequest.yml から actions/github-script 経由で呼ばれる。
// ローカル確認用に、純粋関数（本文の解析・組み立て）も export している。

"use strict";

const BASE_BRANCH = "develop";

// 自動生成本文の目印。これが残っている間は、master へのマージのたびに本文を作り直す。
const MARKER_PREFIX = "<!-- auto-develop-pr";
const MARKER_NOTE = "この行を消すと、以降は本文を自動更新しません（クローズするIssueの追記だけ続けます）";

// テンプレの見出し。key は正規化後の見出し名。
const SECTIONS = {
    check: "必須チェック",
    summary: "概要",
    issues: "クローズするIssue",
    cause: "原因",
    fix: "対処",
    verified: "確認済みの内容",
    unverified: "未確認・残論点",
};

// 必須チェックの項目。元PRの行をこの文言の前方一致で突き合わせる。
const CHECK_ITEMS = [
    "コンパイルエラーが無いことを確認した",
    "変更が関係しうるシーンでPlayModeを最低1回通した",
];

const CLOSING_LINE = "ご確認いただけますと幸いです。";

// ---------------------------------------------------------------------------
// 本文の解析
// ---------------------------------------------------------------------------

/** 改行を LF にそろえ、HTML コメントを取り除く。`<!-- -->` のようにコード中に書かれたものは残す。 */
function stripComments(body) {
    return (body ?? "")
        .replace(/\r\n/g, "\n")
        .replace(/(?<!`)<!--[\s\S]*?-->(?!`)/g, "");
}

/** 見出し文字列をテンプレの key に対応付ける。対応しなければ null。 */
function sectionKeyOf(heading) {
    // 「必須チェック（未チェックの…）」のような括弧書きを落として比較する。
    const name = heading.replace(/[（(].*$/, "").trim();
    if (/issue/i.test(name) && /(クローズ|close|解決)/i.test(name)) return "issues";
    for (const [key, label] of Object.entries(SECTIONS)) {
        if (name === label) return key;
    }
    return null;
}

/**
 * PR 本文を `## 見出し` ごとに分ける。
 * HTML コメント、結びの挨拶、Claude Code の署名行は取り除く。
 * @returns {{ sections: Record<string,string>, extras: {heading:string, body:string}[] }}
 */
function parseBody(body) {
    const text = stripComments(body);

    const sections = {};
    const extras = [];
    let current = null;
    let lines = [];

    const flush = () => {
        const content = cleanSectionBody(lines.join("\n"));
        if (current === null) {
            if (content) extras.push({ heading: "", body: content });
        } else if (current.key) {
            sections[current.key] = sections[current.key]
                ? `${sections[current.key]}\n${content}`
                : content;
        } else if (content) {
            extras.push({ heading: current.heading, body: content });
        }
    };

    for (const line of text.split("\n")) {
        const m = /^##\s+(.+?)\s*$/.exec(line);
        if (m) {
            flush();
            current = { heading: m[1], key: sectionKeyOf(m[1]) };
            lines = [];
        } else {
            lines.push(line);
        }
    }
    flush();
    return { sections, extras };
}

// エージェントが本文末尾に付ける署名・セッションリンクの行。
const FOOTER_PATTERNS = [
    /^🤖 Generated with /,
    /^_?Generated (with|by) \[/,
    /^https:\/\/claude\.ai\/code\/session_\S+$/,
];

function cleanSectionBody(text) {
    const lines = text
        .split("\n")
        .filter((l) => l.trim() !== CLOSING_LINE)
        .filter((l) => !FOOTER_PATTERNS.some((p) => p.test(l.trim())));
    // 署名の区切りとして残った末尾の水平線を落とす。
    while (lines.length > 0 && /^(\s*|-{3,}|\*{3,})$/.test(lines[lines.length - 1].trim())) lines.pop();
    return lines.join("\n").replace(/\n{3,}/g, "\n\n").trim();
}

/** 必須チェック節から、項目ごとのチェック状態と補足を取り出す。 */
function parseChecks(checkSection) {
    const result = {};
    for (const line of (checkSection ?? "").split("\n")) {
        const m = /^\s*[-*]\s*\[([ xX])\]\s*(.+)$/.exec(line);
        if (!m) continue;
        const item = CHECK_ITEMS.find((i) => m[2].startsWith(i));
        if (!item) continue;
        const note = m[2].slice(item.length).trim().replace(/^[（(](.*)[）)]$/, "$1").trim();
        result[item] = { checked: m[1] !== " ", note };
    }
    return result;
}

/**
 * Issue 参照を取り出す。
 * - 「クローズするIssue」節の中は、キーワードなしの `#N` もクローズ対象とみなす。
 * - それ以外の節では、`Closes #N` などクローズ用のキーワードが付いたものだけを拾う。
 * @returns {{owner:string, repo:string, number:number}[]}
 */
function extractIssueRefs(body, repo) {
    const text = stripComments(body);
    const { sections } = parseBody(body);

    const refPattern = String.raw`(?:https://github\.com/([\w.-]+)/([\w.-]+)/issues/(\d+)|(?:([\w.-]+)/([\w.-]+))?#(\d+))`;
    const toRef = (m, offset) => ({
        owner: m[offset] ?? m[offset + 3] ?? repo.owner,
        repo: m[offset + 1] ?? m[offset + 4] ?? repo.repo,
        number: Number(m[offset + 2] ?? m[offset + 5]),
    });

    const refs = [];
    for (const m of (sections.issues ?? "").matchAll(new RegExp(refPattern, "g"))) {
        refs.push(toRef(m, 1));
    }
    const keyword = String.raw`\b(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\b:?\s+`;
    for (const m of text.matchAll(new RegExp(keyword + refPattern, "gi"))) {
        refs.push(toRef(m, 1));
    }
    return dedupeRefs(refs);
}

function dedupeRefs(refs) {
    const seen = new Set();
    return refs.filter((r) => {
        const k = `${r.owner}/${r.repo}#${r.number}`.toLowerCase();
        if (seen.has(k)) return false;
        seen.add(k);
        return true;
    });
}

function formatRef(ref, repo) {
    const same = ref.owner.toLowerCase() === repo.owner.toLowerCase()
        && ref.repo.toLowerCase() === repo.repo.toLowerCase();
    return same ? `#${ref.number}` : `${ref.owner}/${ref.repo}#${ref.number}`;
}

// ---------------------------------------------------------------------------
// 本文の組み立て
// ---------------------------------------------------------------------------

/**
 * @param {object} args
 * @param {string} args.head master ブランチ名
 * @param {{owner:string, repo:string}} args.repo
 * @param {{number:number, title:string, body:string, user:string}[]} args.pulls 取り込む PR（マージ順）
 * @param {{ref:object, title?:string, state?:string, skipped?:string}[]} args.issues 検証済みの Issue
 * @param {{totalCommits:number, commits:{sha:string, message:string}[], files:{filename:string, additions:number, deletions:number}[], filesTruncated:boolean}} args.diff
 * @returns {{ title: string, body: string }}
 */
function buildPullRequest({ head, repo, pulls, issues, diff }) {
    const parsed = pulls.map((p) => ({ pull: p, ...parseBody(p.body) }));
    const multi = parsed.length > 1;
    const out = [];

    const title = parsed.length === 0
        ? `${head} → ${BASE_BRANCH}`
        : multi
            ? `${parsed[0].pull.title} ほか${parsed.length - 1}件`
            : parsed[0].pull.title;

    out.push(`${MARKER_PREFIX} ${JSON.stringify({ title })} ${MARKER_NOTE} -->`);
    out.push("");

    // 必須チェック: 全ての元PRでチェック済みの項目だけ [x] にする。
    out.push(`## ${SECTIONS.check}（未チェックのPRは提出しないでください）`);
    const uncheckedNotes = [];
    for (const item of CHECK_ITEMS) {
        const states = parsed.map((p) => ({ pull: p.pull, state: parseChecks(p.sections.check)[item] }));
        const allChecked = states.length > 0 && states.every((s) => s.state?.checked);
        out.push(`- [${allChecked ? "x" : " "}] ${item}`);
        for (const s of states) {
            if (s.state?.note) out.push(`  - ${multi ? `#${s.pull.number}: ` : ""}${s.state.note}`);
        }
        const unchecked = states.filter((s) => !s.state?.checked).map((s) => `#${s.pull.number}`);
        if (unchecked.length > 0) uncheckedNotes.push(`- 「${item}」が未チェックの元PRがあります: ${unchecked.join(", ")}`);
    }
    out.push("");

    out.push(`## ${SECTIONS.summary}`);
    out.push(mergeSection(parsed, "summary", multi) || fallbackSummary(head, diff));
    out.push("");

    out.push(`## ${SECTIONS.issues}`);
    out.push(formatIssues(issues, repo));
    out.push("");

    out.push(`## ${SECTIONS.cause}`);
    out.push(mergeSection(parsed, "cause", multi) || "なし。");
    out.push("");

    out.push(`## ${SECTIONS.fix}`);
    out.push(mergeSection(parsed, "fix", multi) || "元PRに記載なし。");
    out.push("");

    out.push(`## ${SECTIONS.verified}`);
    out.push(mergeSection(parsed, "verified", multi) || "元PRに記載なし。");
    out.push("");

    out.push(`## ${SECTIONS.unverified}`);
    if (parsed.length === 0) uncheckedNotes.push("- 元になる PR が見つからないため、本文はコミット一覧から作っています。内容を追記してください。");
    const unverified = [mergeSection(parsed, "unverified", multi), uncheckedNotes.join("\n")].filter(Boolean);
    out.push(unverified.join("\n\n") || "元PRに記載なし。");
    out.push("");

    const extras = parsed.flatMap((p) => p.extras.map((e) => ({ ...e, pull: p.pull })));
    if (extras.length > 0) {
        out.push("## その他（元PRのテンプレ外の記載）");
        for (const e of extras) {
            out.push(`### ${e.heading || "（見出しなし）"}${multi ? `（#${e.pull.number}）` : ""}`);
            out.push(e.body);
            out.push("");
        }
    }

    out.push("## 取り込む変更");
    out.push(formatSources(parsed, diff));
    out.push("");

    out.push(CLOSING_LINE);
    return { title, body: out.join("\n") };
}

/** 複数 PR の同じ節をまとめる。1件ならそのまま、複数なら PR ごとに小見出しを付ける。 */
function mergeSection(parsed, key, multi) {
    const parts = parsed.filter((p) => p.sections[key]);
    if (parts.length === 0) return "";
    if (!multi) return parts[0].sections[key];
    return parts.map((p) => `#### #${p.pull.number} ${p.pull.title}\n${p.sections[key]}`).join("\n\n");
}

function fallbackSummary(head, diff) {
    const lines = [`${head} の変更を ${BASE_BRANCH} に取り込みます。元になる PR が無いため、コミットの一覧を載せます。`];
    for (const c of diff.commits.slice(0, 20)) lines.push(`- ${c.message.split("\n")[0]}`);
    if (diff.commits.length > 20) lines.push(`- ほか${diff.commits.length - 20}件`);
    return lines.join("\n");
}

function formatIssues(issues, repo) {
    const closable = issues.filter((i) => !i.skipped);
    const skipped = issues.filter((i) => i.skipped);
    const lines = [];
    if (closable.length === 0) {
        lines.push("なし。");
    } else {
        lines.push("このPRを develop にマージすると、次の Issue が自動でクローズされます。");
        for (const i of closable) {
            const suffix = [i.title, i.state === "closed" ? "（クローズ済み）" : ""].filter(Boolean).join(" ");
            lines.push(`- Closes ${formatRef(i.ref, repo)}${suffix ? ` ${suffix}` : ""}`);
        }
    }
    for (const i of skipped) {
        lines.push(`- ${formatRef(i.ref, repo)} は対象外にしました（${i.skipped}）`);
    }
    return lines.join("\n");
}

function formatSources(parsed, diff) {
    const lines = [];
    if (parsed.length > 0) {
        lines.push("元PR（この Draft PR の本文は、次の PR から自動で組み立てています）:");
        for (const p of parsed) lines.push(`- #${p.pull.number} ${p.pull.title}${p.pull.user ? ` (@${p.pull.user})` : ""}`);
    }

    const additions = diff.files.reduce((s, f) => s + f.additions, 0);
    const deletions = diff.files.reduce((s, f) => s + f.deletions, 0);
    lines.push(`- コミット ${diff.totalCommits}件 / 変更ファイル ${diff.files.length}${diff.filesTruncated ? "件以上" : "件"} (+${additions} / -${deletions})`);

    // どの領域に手が入ったかを、パスの上位2階層で集計する。
    const areas = new Map();
    for (const f of diff.files) {
        const area = f.filename.split("/").slice(0, 2).join("/");
        areas.set(area, (areas.get(area) ?? 0) + 1);
    }
    const top = [...areas.entries()].sort((a, b) => b[1] - a[1]);
    if (top.length > 0) {
        const shown = top.slice(0, 8).map(([a, n]) => `\`${a}\` ${n}`);
        if (top.length > 8) shown.push(`ほか${top.length - 8}箇所`);
        lines.push(`- 変更箇所: ${shown.join(" / ")}`);
    }
    return lines.join("\n");
}

/** 目印が消された（手で書き直された）本文に、足りない `Closes` 行だけを追記する。 */
function appendMissingClosings(body, issues, repo) {
    const existing = extractIssueRefs(body, repo);
    const has = (ref) => existing.some((e) =>
        e.number === ref.number && e.owner.toLowerCase() === ref.owner.toLowerCase() && e.repo.toLowerCase() === ref.repo.toLowerCase());
    const missing = issues.filter((i) => !i.skipped && !has(i.ref));
    if (missing.length === 0) return null;
    const lines = missing.map((i) => `Closes ${formatRef(i.ref, repo)}`);
    return `${body.replace(/\s+$/, "")}\n\n<!-- master に後から取り込まれた PR の Issue を自動で追記 -->\n${lines.join("\n")}\n`;
}

function readMarker(body) {
    const line = (body ?? "").split(/\r?\n/).find((l) => l.startsWith(MARKER_PREFIX));
    if (!line) return null;
    const m = /^<!-- auto-develop-pr (\{.*?\}) /.exec(line);
    try {
        return m ? JSON.parse(m[1]) : {};
    } catch {
        return {};
    }
}

// ---------------------------------------------------------------------------
// GitHub API とのやり取り
// ---------------------------------------------------------------------------

/** master に取り込まれていて、まだ develop に入っていない PR をマージ順に返す。 */
async function collectPulls(github, repo, head) {
    const closed = await github.paginate(github.rest.pulls.list, {
        ...repo, base: head, state: "closed", per_page: 100,
    });
    const merged = closed.filter((p) => p.merged_at && p.merge_commit_sha);

    const pulls = [];
    for (const p of merged) {
        // develop が merge_commit_sha を含んでいれば、前回までに取り込み済み。
        const { data } = await github.rest.repos.compareCommitsWithBasehead({
            ...repo, basehead: `${BASE_BRANCH}...${p.merge_commit_sha}`, per_page: 1,
        });
        if (data.status === "ahead" || data.status === "diverged") pulls.push(p);
    }
    pulls.sort((a, b) => new Date(a.merged_at) - new Date(b.merged_at));
    return pulls.map((p) => ({ number: p.number, title: p.title, body: p.body ?? "", user: p.user?.login }));
}

async function collectDiff(github, repo, head) {
    const { data } = await github.rest.repos.compareCommitsWithBasehead({
        ...repo, basehead: `${BASE_BRANCH}...${head}`,
    });
    return {
        totalCommits: data.total_commits,
        commits: data.commits.map((c) => ({ sha: c.sha, message: c.commit.message })),
        files: (data.files ?? []).map((f) => ({ filename: f.filename, additions: f.additions, deletions: f.deletions })),
        // compare API はファイルを最大300件までしか返さない。
        filesTruncated: (data.files ?? []).length >= 300,
    };
}

/** Issue が実在し、PR ではないことを確かめる。 */
async function resolveIssues(github, refs) {
    const issues = [];
    for (const ref of refs) {
        try {
            const { data } = await github.rest.issues.get({ owner: ref.owner, repo: ref.repo, issue_number: ref.number });
            if (data.pull_request) {
                issues.push({ ref, skipped: "Issue ではなく PR" });
            } else {
                issues.push({ ref, title: data.title, state: data.state });
            }
        } catch (e) {
            issues.push({ ref, skipped: e.status === 404 ? "見つからない" : `取得に失敗: ${e.status ?? e.message}` });
        }
    }
    return issues;
}

/**
 * master → develop の Draft PR を作る、または本文を更新する。
 * @param {object} args
 * @param {boolean} [args.dryRun] true なら作成・更新をせず、本文を返すだけにする
 */
async function run({ github, context, core, masterBranch, dryRun = false }) {
    const repo = { owner: context.repo.owner, repo: context.repo.repo };
    const head = masterBranch;

    const pulls = await collectPulls(github, repo, head);
    const diff = await collectDiff(github, repo, head);
    if (diff.totalCommits === 0) {
        core.notice(`${head} と ${BASE_BRANCH} の間にコミットが無いため、Draft PR は不要です。`);
        return { action: "none" };
    }

    const refs = dedupeRefs(pulls.flatMap((p) => extractIssueRefs(p.body, repo)));
    const issues = await resolveIssues(github, refs);
    const { title, body } = buildPullRequest({ head, repo, pulls, issues, diff });

    const existing = (await github.rest.pulls.list({
        ...repo, head: `${repo.owner}:${head}`, base: BASE_BRANCH, state: "open",
    })).data[0];

    if (dryRun) return { action: existing ? "update" : "create", title, body, existing: existing?.number };

    if (!existing) {
        try {
            const { data } = await github.rest.pulls.create({ ...repo, title, head, base: BASE_BRANCH, body, draft: true });
            core.info(`Created draft pull request: ${data.html_url}`);
            return { action: "create", number: data.number };
        } catch (e) {
            const details = JSON.stringify(e.response?.data ?? {});
            if (e.status === 403) {
                core.warning(`GitHub Actions に PR 作成の権限が無いため、Draft PR を作れませんでした: ${details}`);
                return { action: "none" };
            }
            if (e.status === 422 && details.includes("No commits between")) {
                core.notice(`${head} と ${BASE_BRANCH} の間にコミットが無いため、Draft PR は不要です。`);
                return { action: "none" };
            }
            throw e;
        }
    }

    const marker = readMarker(existing.body);
    if (marker) {
        // タイトルは、前回自動で付けたものか既定の形のままなら更新する。手で変えていれば残す。
        const titleIsAuto = existing.title === marker.title || existing.title === `${head} → ${BASE_BRANCH}`;
        await github.rest.pulls.update({
            ...repo, pull_number: existing.number, body, ...(titleIsAuto ? { title } : {}),
        });
        core.info(`Updated pull request body: ${existing.html_url}`);
        return { action: "update", number: existing.number };
    }

    const appended = appendMissingClosings(existing.body ?? "", issues, repo);
    if (appended) {
        await github.rest.pulls.update({ ...repo, pull_number: existing.number, body: appended });
        core.info(`Appended closing keywords: ${existing.html_url}`);
        return { action: "append", number: existing.number };
    }
    core.info(`本文は手で書き直されているため、更新しません: ${existing.html_url}`);
    return { action: "none", number: existing.number };
}

module.exports = {
    run,
    parseBody,
    parseChecks,
    extractIssueRefs,
    buildPullRequest,
    appendMissingClosings,
    readMarker,
};
