// 使われなくなった feature/<段階>/<作業名>/master を検出し、削除する（dryRun なら報告だけ）。
// CleanupStaleBranches.yml から actions/github-script 経由で呼ばれる。
// ローカル確認用に、純粋関数（ブランチの分類）も export している。

"use strict";

const BASE_BRANCH = "develop";

// 判定結果の種類。Job Summary の見出しにも使う。
const VERDICTS = {
    delete: "削除",
    review: "要確認（develop 未取り込みのコミットあり）",
    keep: "対象外",
};

// ---------------------------------------------------------------------------
// ブランチの分類
// ---------------------------------------------------------------------------

/** `feature/<段階>/<作業名>/master` なら階層（末尾の / まで）を返す。それ以外は null。 */
function masterGroupOf(branch) {
    const m = /^(feature\/.+\/)master$/.exec(branch);
    if (!m) return null;
    // AutoCreateMasterBranch.yml と同じく、4層以上（/ が3つ以上）のものだけを扱う。
    return (branch.match(/\//g) ?? []).length >= 3 ? m[1] : null;
}

/** ブランチが属する階層（親ディレクトリ + /）を返す。 */
function groupOf(branch) {
    const i = branch.lastIndexOf("/");
    return i < 0 ? "" : branch.slice(0, i + 1);
}

/**
 * master ごとに、同階層に残っている master 以外のブランチを集める。
 * @param {string[]} branches リモートの全ブランチ名
 * @returns {{ master: string, siblings: string[] }[]}
 */
function collectMasters(branches) {
    const masters = branches
        .map((name) => ({ name, group: masterGroupOf(name) }))
        .filter((m) => m.group !== null);
    return masters.map(({ name, group }) => ({
        master: name,
        // 同じ階層の直下にあるブランチだけを兄弟とみなす（さらに深い階層は別の作業）。
        siblings: branches.filter((b) => b !== name && groupOf(b) === group),
    }));
}

/**
 * 1本の master について、削除してよいかを決める。
 * @param {{ siblings: string[], openPulls: number[], aheadBy: number }} state
 * @returns {{ verdict: keyof VERDICTS, reason: string }}
 */
function judge({ siblings, openPulls, aheadBy }) {
    if (siblings.length > 0) {
        return { verdict: "keep", reason: `同階層にブランチがある: ${siblings.join(", ")}` };
    }
    if (openPulls.length > 0) {
        return { verdict: "keep", reason: `open PR がある: ${openPulls.map((n) => `#${n}`).join(", ")}` };
    }
    if (aheadBy > 0) {
        return { verdict: "review", reason: `${BASE_BRANCH} に無いコミットが ${aheadBy} 件ある` };
    }
    return { verdict: "delete", reason: `名前ブランチが無く、${BASE_BRANCH} との差分も無い` };
}

// ---------------------------------------------------------------------------
// GitHub API
// ---------------------------------------------------------------------------

/** open PR の head / base ごとに PR 番号を引けるようにする。 */
async function collectOpenPulls(github, repo) {
    const pulls = await github.paginate(github.rest.pulls.list, { ...repo, state: "open", per_page: 100 });
    const byBranch = new Map();
    const add = (branch, number) => byBranch.set(branch, [...(byBranch.get(branch) ?? []), number]);
    for (const p of pulls) {
        // フォークからの PR は head が別リポジトリなので、このリポジトリのブランチとは突き合わせない。
        if (p.head.repo?.full_name === `${repo.owner}/${repo.repo}`) add(p.head.ref, p.number);
        add(p.base.ref, p.number);
    }
    return byBranch;
}

/** develop に無く master にだけあるコミットの数。 */
async function aheadOf(github, repo, branch) {
    const { data } = await github.rest.repos.compareCommitsWithBasehead({
        ...repo, basehead: `${BASE_BRANCH}...${branch}`, per_page: 1,
    });
    return data.ahead_by;
}

/** Job Summary 用の表を組み立てる。 */
function buildSummary(results, { dryRun, scope }) {
    const lines = [
        `## 不要ブランチの整理（${dryRun ? "dry-run: 削除していません" : "削除を実行"}）`,
        "",
        `- 対象: ${scope ?? "すべての feature/<段階>/<作業名>/master"}`,
        "",
    ];
    for (const [verdict, label] of Object.entries(VERDICTS)) {
        const rows = results.filter((r) => r.verdict === verdict);
        lines.push(`### ${label}（${rows.length}件）`, "");
        if (rows.length === 0) {
            lines.push("なし", "");
            continue;
        }
        lines.push("| ブランチ | 理由 | 結果 |", "| --- | --- | --- |");
        for (const r of rows) lines.push(`| \`${r.master}\` | ${r.reason} | ${r.outcome ?? "-"} |`);
        lines.push("");
    }
    return lines.join("\n");
}

/**
 * 不要な master を判定し、dryRun でなければ削除する。
 * @param {object} args
 * @param {boolean} args.dryRun true なら削除せず、判定結果を報告するだけにする
 * @param {string} [args.deletedBranch] delete イベントで消されたブランチ。指定時はその階層の master だけを見る
 */
async function run({ github, context, core, dryRun, deletedBranch }) {
    const repo = { owner: context.repo.owner, repo: context.repo.repo };

    const branches = (await github.paginate(github.rest.repos.listBranches, { ...repo, per_page: 100 }))
        .map((b) => b.name);
    const scopeGroup = deletedBranch ? groupOf(deletedBranch) : null;
    const targets = collectMasters(branches)
        .filter((m) => scopeGroup === null || groupOf(m.master) === scopeGroup);
    if (scopeGroup !== null && targets.length === 0) {
        core.notice(`${scopeGroup}master は存在しないため、整理する対象はありません。`);
    }

    const openPulls = await collectOpenPulls(github, repo);
    const results = [];
    for (const { master, siblings } of targets) {
        const pulls = openPulls.get(master) ?? [];
        // 兄弟や PR で残すと決まったものは compare を呼ばずに済ませる。
        const aheadBy = siblings.length === 0 && pulls.length === 0 ? await aheadOf(github, repo, master) : 0;
        const result = { master, ...judge({ siblings, openPulls: pulls, aheadBy }) };

        if (result.verdict === "delete") {
            if (dryRun) {
                result.outcome = "削除候補";
            } else {
                try {
                    await github.rest.git.deleteRef({ ...repo, ref: `heads/${master}` });
                    result.outcome = "削除した";
                    core.info(`Deleted branch: ${master}`);
                } catch (e) {
                    result.outcome = `削除に失敗: ${e.status ?? e.message}`;
                    core.warning(`${master} を削除できませんでした: ${e.status ?? e.message}`);
                }
            }
        }
        results.push(result);
    }

    await core.summary.addRaw(buildSummary(results, { dryRun, scope: scopeGroup ? `${scopeGroup}master` : null })).write();
    return results;
}

module.exports = {
    run,
    masterGroupOf,
    collectMasters,
    judge,
    buildSummary,
};
