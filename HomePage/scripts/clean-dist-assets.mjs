// astro:assetsのimportが、getImage()経由でしか使っていない画像でも
// 元ファイルをdist/_astro/に静的アセットとしてコピーしてしまうことがあるため、
// ビルド後にどのHTML/CSS/JSからも参照されていないファイルを検出して削除する。
import { readFileSync, readdirSync, rmSync, statSync } from 'node:fs';
import { extname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const distDir = fileURLToPath(new URL('../dist/', import.meta.url));
const astroDir = join(distDir, '_astro');

const TEXT_EXTENSIONS = new Set(['.html', '.css', '.js', '.mjs', '.json', '.xml', '.svg', '.txt']);

function walk(dir) {
	const files = [];
	for (const entry of readdirSync(dir, { withFileTypes: true })) {
		const full = join(dir, entry.name);
		if (entry.isDirectory()) {
			files.push(...walk(full));
		} else {
			files.push(full);
		}
	}
	return files;
}

const allFiles = walk(distDir);
const textFiles = allFiles.filter((file) => TEXT_EXTENSIONS.has(extname(file)));
const haystack = textFiles.map((file) => readFileSync(file, 'utf-8')).join('\n');

const candidates = allFiles.filter((file) => file.startsWith(astroDir) && !TEXT_EXTENSIONS.has(extname(file)));

let removedCount = 0;
let removedBytes = 0;

for (const file of candidates) {
	const basename = file.split('/').pop();
	// 日本語ファイル名等はHTML/CSS上でURLエンコードされて出力されるため、
	// 生のファイル名とエンコード後の両方で参照有無を確認する。
	const encodedBasename = encodeURIComponent(basename);
	if (haystack.includes(basename) || haystack.includes(encodedBasename)) continue;

	removedBytes += statSync(file).size;
	rmSync(file);
	removedCount += 1;
	console.log(`[clean-dist-assets] removed unreferenced asset: ${basename}`);
}

if (removedCount > 0) {
	console.log(`[clean-dist-assets] removed ${removedCount} file(s), ${(removedBytes / 1024 / 1024).toFixed(2)}MB total`);
} else {
	console.log('[clean-dist-assets] no unreferenced assets found');
}
