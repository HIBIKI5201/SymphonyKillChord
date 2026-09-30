import { defineCollection } from 'astro:content';
import { glob } from 'astro/loaders';
import { z } from 'astro/zod';

// お知らせは src/content/news/ に1件1ファイルのMarkdownで置く。ファイル名が記事のURL（news/<ファイル名>）になる。
const news = defineCollection({
	loader: glob({ pattern: '**/*.md', base: './src/content/news' }),
	schema: z.object({
		title: z.string(),
		// 一覧の並び順と表示に使う。元になった記事・出来事の日付を書く。
		date: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
		// 記事末尾に並べる外部リンク。別タブで開く。
		links: z
			.array(
				z.object({
					label: z.string(),
					href: z.string().url(),
				}),
			)
			.default([]),
	}),
});

export const collections = { news };
