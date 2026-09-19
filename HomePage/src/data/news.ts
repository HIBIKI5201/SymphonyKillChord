export interface NewsItem {
	date: string;
	title: string;
	href: string;
}

// お知らせを追加すると、ホームと一覧ページの両方に新しい順で表示される。
export const news: NewsItem[] = [];

export const sortedNews = [...news].sort((a, b) => b.date.localeCompare(a.date));
