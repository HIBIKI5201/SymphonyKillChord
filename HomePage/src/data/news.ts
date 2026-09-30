import { getCollection, type CollectionEntry } from 'astro:content';

export type NewsItem = CollectionEntry<'news'>;

// お知らせの本文は src/content/news/ の Markdown に書く。ここでは新しい順に並べて返すだけ。
export const getSortedNews = async (): Promise<NewsItem[]> =>
	(await getCollection('news')).sort((a, b) => b.data.date.localeCompare(a.data.date));

export const newsHref = (item: NewsItem) => `${import.meta.env.BASE_URL}news/${item.id}`;
