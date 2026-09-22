// Notion 公開ページ クローラ (認証不要)。loadPageChunk / syncRecordValuesMain / queryCollection を使う。
// 使い方: node crawl-public-notion.cjs [出力ディレクトリ(既定: OS の一時ディレクトリ/notion-snapshot-<日付>)]
// 出力: <出力>/NNNN-<タイトル>-<id末尾>.md (1ページ1ファイル), <出力>/_INDEX.tsv, <出力>/notion-index.json
// タスクリスト・時間割・休暇日・スプリント DB の行ページは辿らない。
// 注意: Notion 非公式 API のため仕様変更で動かなくなることがある。NOTION_TOKEN があるなら NotionMarkdownExporter を優先する。
const fs = require('fs');
const path = require('path');
const SITE = 'https://lying-foxglove-81a.notion.site/api/v3';
const ROOT = '27d7c2c6-cc02-801d-9648-fbe2769f1971';
// 出力先の既定値は OS の一時ディレクトリにする（リポジトリの中に書き出して誤ってコミットしないため）。
const OUT = path.resolve(process.argv[2] || path.join(require('os').tmpdir(), 'notion-snapshot-' + new Date().toISOString().slice(0, 10)));
console.log('出力先: ' + OUT);
const SKIP_DB = new Set(); // 行を辿らない DB の id (名前による除外は下の正規表現)
fs.mkdirSync(OUT, { recursive: true });

const blocks = new Map();
const collections = new Map();
async function post(ep, body, tries = 5) {
  for (let i = 0; i < tries; i++) {
    try {
      const r = await fetch(`${SITE}/${ep}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body) });
      if (r.status === 429) { await new Promise(s => setTimeout(s, 2000 * (i + 1))); continue; }
      if (!r.ok) throw new Error(`${ep} ${r.status}`);
      return await r.json();
    } catch (e) { if (i === tries - 1) throw e; await new Promise(s => setTimeout(s, 1000 * (i + 1))); }
  }
}
const val = v => (v && v.value && v.value.value) ? v.value.value : (v && v.value) ? v.value : v;
function absorb(rm) {
  for (const [k, v] of Object.entries(rm.block || {})) { const x = val(v); if (x && x.id) blocks.set(k, x); }
  for (const [k, v] of Object.entries(rm.collection || {})) { const x = val(v); if (x && x.id) collections.set(k, x); }
}
async function loadPage(id) {
  let cursor = { stack: [] }, chunk = 0;
  do {
    const j = await post('loadPageChunk', { pageId: id, limit: 300, cursor, chunkNumber: chunk++, verticalColumns: false });
    absorb(j.recordMap || {});
    cursor = j.cursor;
  } while (cursor && cursor.stack && cursor.stack.length);
}
async function fillMissing(ids) {
  const miss = ids.filter(i => !blocks.has(i));
  for (let i = 0; i < miss.length; i += 100) {
    const j = await post('syncRecordValuesMain', { requests: miss.slice(i, i + 100).map(id => ({ pointer: { table: 'block', id }, version: -1 })) }).catch(e => { console.error('MISS', e.message); return {}; });
    absorb(j.recordMap || {});
  }
}
async function queryDb(b) {
  const collId = b.collection_id || (b.format && b.format.collection_pointer && b.format.collection_pointer.id);
  const viewId = (b.view_ids || [])[0];
  if (!collId || !viewId) return { collId, rows: [] };
  if (!collections.has(collId)) {
    const j = await post('syncRecordValuesMain', { requests: [{ pointer: { table: 'collection', id: collId }, version: -1 }] });
    absorb(j.recordMap || {});
  }
  const j = await post('queryCollection', {
    collection: { id: collId }, collectionView: { id: viewId },
    loader: { type: 'reducer', reducers: { collection_group_results: { type: 'results', limit: 1000 } }, searchQuery: '', userTimeZone: 'Asia/Tokyo' }
  }).catch(e => ({ error: String(e) }));
  if (j.error) return { collId, rows: [], error: j.error };
  absorb(j.recordMap || {});
  const ids = (j.result && j.result.reducerResults && j.result.reducerResults.collection_group_results && j.result.reducerResults.collection_group_results.blockIds) || [];
  return { collId, rows: ids };
}

// ---- rendering ----
function rt(t) {
  if (!t) return '';
  return t.map(([s, marks]) => {
    let o = s;
    for (const m of marks || []) {
      if (m[0] === 'b') o = `**${o}**`; else if (m[0] === 'i') o = `*${o}*`; else if (m[0] === 'c') o = '`' + o + '`';
      else if (m[0] === 's') o = `~~${o}~~`; else if (m[0] === 'a') o = `[${o}](${m[1]})`;
      else if (m[0] === 'p') { const r = blocks.get(m[1]); o = `[[${r ? rt(r.properties && r.properties.title) : m[1]}]]`; }
      else if (m[0] === 'd') o = (m[1] && m[1].start_date) || o;
      else if (m[0] === 'u') o = '@user';
    }
    return o;
  }).join('');
}
function propText(coll, props) {
  if (!coll || !props) return [];
  const out = [];
  for (const [pid, sch] of Object.entries(coll.schema || {})) {
    if (pid === 'title') continue;
    const v = props[pid];
    if (v) out.push(`${sch.name}: ${rt(v).replace(/\n/g, ' ')}`);
  }
  return out;
}
const pages = []; // {id,title,path,parent}
const dbs = [];
const seen = new Set();
function fname(t) { return (t || 'untitled').replace(/[\\/:*?"<>|\n\r]/g, '_').slice(0, 60); }

async function renderChildren(ids, depth, lines, ctx) {
  await fillMissing(ids || []);
  let numIdx = 0;
  for (const id of ids || []) {
    const b = blocks.get(id);
    if (!b || b.alive === false) continue;
    const ind = '  '.repeat(depth);
    const title = rt(b.properties && b.properties.title);
    if (b.type !== 'numbered_list') numIdx = 0;
    switch (b.type) {
      case 'page':
        lines.push(`${ind}- 📄 [[${title}]] (${id})`);
        ctx.childPages.push(id); continue;
      case 'collection_view_page': case 'collection_view': {
        const q = await queryDb(b);
        const c = collections.get(q.collId);
        const name = c ? rt(c.name) : '(db)';
        lines.push(`${ind}- 🗃 DB「${name}」 (${id}) rows=${q.rows.length}${q.error ? ' ERROR ' + q.error : ''}`);
        dbs.push({ id, name, rows: q.rows.length, parentPage: ctx.pageId });
        const skip = SKIP_DB.has(id) || SKIP_DB.has(q.collId) || /^(タスクリスト|時間割|休暇日|スプリント)/.test(name);
        for (const r of q.rows) {
          const rb = blocks.get(r); if (!rb) continue;
          lines.push(`${ind}  - [[${rt(rb.properties && rb.properties.title)}]] (${r}) ${propText(c, rb.properties).join(' | ')}`);
          if (!skip && rb.content && rb.content.length) ctx.childPages.push(r);
        }
        continue;
      }
      case 'header': lines.push(`\n${'#'.repeat(Math.min(6, 2 + ctx.h))} ${title}`); break;
      case 'sub_header': lines.push(`\n${'#'.repeat(Math.min(6, 3 + ctx.h))} ${title}`); break;
      case 'sub_sub_header': lines.push(`\n${'#'.repeat(Math.min(6, 4 + ctx.h))} ${title}`); break;
      case 'text': lines.push(`${ind}${title}`); break;
      case 'bulleted_list': lines.push(`${ind}- ${title}`); break;
      case 'numbered_list': lines.push(`${ind}${++numIdx}. ${title}`); break;
      case 'to_do': lines.push(`${ind}- [${b.properties && b.properties.checked ? 'x' : ' '}] ${title}`); break;
      case 'toggle': lines.push(`${ind}- ▶ ${title}`); break;
      case 'quote': lines.push(`${ind}> ${title}`); break;
      case 'callout': lines.push(`${ind}> 💡 ${title}`); break;
      case 'code': lines.push('```' + ((b.properties && b.properties.language && rt(b.properties.language)) || '') + '\n' + title + '\n```'); break;
      case 'divider': lines.push('---'); break;
      case 'image': case 'video': case 'file': case 'pdf': case 'audio':
        lines.push(`${ind}[${b.type}: ${rt(b.properties && b.properties.caption) || rt(b.properties && b.properties.title) || ''}] ${(b.properties && b.properties.source && rt(b.properties.source)) || ''}`); break;
      case 'bookmark': case 'embed': lines.push(`${ind}[${b.type}] ${rt(b.properties && b.properties.link) || rt(b.properties && b.properties.source)}`); break;
      case 'table': {
        const cols = (b.format && b.format.table_block_column_order) || [];
        await fillMissing(b.content || []);
        const rows = (b.content || []).map(r => blocks.get(r)).filter(Boolean);
        rows.forEach((r, i) => {
          lines.push('| ' + cols.map(c => rt(r.properties && r.properties[c]).replace(/\|/g, '/').replace(/\n/g, ' ')).join(' | ') + ' |');
          if (i === 0) lines.push('|' + cols.map(() => '---').join('|') + '|');
        });
        continue;
      }
      case 'column_list': case 'column': case 'transclusion_container': await renderChildren(b.content, depth, lines, ctx); continue;
      case 'transclusion_reference': {
        const ref = b.format && b.format.transclusion_reference_pointer && b.format.transclusion_reference_pointer.id;
        if (ref) { await fillMissing([ref]); const rb = blocks.get(ref); if (rb) await renderChildren(rb.content, depth, lines, ctx); }
        continue;
      }
      case 'alias': { const ref = b.format && b.format.alias_pointer && b.format.alias_pointer.id; lines.push(`${ind}- ↪ link (${ref})`); continue; }
      case 'table_of_contents': case 'breadcrumb': continue;
      default: lines.push(`${ind}[${b.type}] ${title}`);
    }
    if (b.content && b.content.length) await renderChildren(b.content, depth + 1, lines, ctx);
  }
}

async function crawl(id, crumbs) {
  if (seen.has(id)) return; seen.add(id);
  try { await loadPage(id); } catch (e) { console.error('FAIL', id, e.message); pages.push({ id, title: '(fail)', crumbs, error: e.message }); return; }
  const b = blocks.get(id);
  if (!b) { pages.push({ id, title: '(missing)', crumbs }); return; }
  const title = rt(b.properties && b.properties.title) || '(untitled)';
  const lines = [`# ${title}`, '', `- id: ${id}`, `- path: ${[...crumbs, title].join(' / ')}`, `- last_edited: ${new Date(b.last_edited_time).toISOString()}`, ''];
  if (b.parent_table === 'collection') { const c = collections.get(b.parent_id); if (c) lines.push(...propText(c, b.properties).map(s => `- ${s}`), ''); }
  const ctx = { childPages: [], pageId: id, h: 0 };
  await renderChildren(b.content, 0, lines, ctx);
  const idx = pages.length + 1;
  const file = `${String(idx).padStart(4, '0')}-${fname(title)}-${id.slice(-8)}.md`;
  fs.writeFileSync(path.join(OUT, file), lines.join('\n'));
  pages.push({ id, title, crumbs, file, last_edited: b.last_edited_time, chars: lines.join('\n').length });
  process.stdout.write(`${idx} ${[...crumbs, title].join('/')}\n`);
  for (const c of ctx.childPages) await crawl(c, [...crumbs, title]);
}
crawl(ROOT, []).then(() => {
  fs.writeFileSync(path.join(OUT, 'notion-index.json'), JSON.stringify({ pages, dbs }, null, 1));
  const tsv = pages.map(p => [p.file, p.last_edited ? new Date(p.last_edited).toISOString().slice(0, 10) : '', p.chars || 0, [...p.crumbs, p.title].join(' / ')].join('\t'));
  fs.writeFileSync(path.join(OUT, '_INDEX.tsv'), 'file\tlast_edited\tchars\tpath\n' + tsv.join('\n'));
  console.log('DONE', pages.length, 'pages', dbs.length, 'dbs');
});
