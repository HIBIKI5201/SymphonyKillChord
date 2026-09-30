export interface NewsLink {
	label: string;
	href: string;
}

export interface NewsItem {
	// 記事ページのURL（news/<slug>）になる。公開後は変えない。
	slug: string;
	date: string;
	title: string;
	// 本文の段落。1要素が1段落になる。
	body: string[];
	// 記事末尾に並べる外部リンク。別タブで開く。
	links: NewsLink[];
}

// お知らせを追加すると、ホームと一覧ページの両方に新しい順で表示され、記事ページも生成される。
export const news: NewsItem[] = [
	{
		slug: 'tgs2026',
		date: '2026-09-17',
		title: '東京ゲームショウ2026に出展しています',
		body: [
			'本日2026年9月17日から21日まで幕張メッセで開催される「東京ゲームショウ2026」にて、バンタンゲームアカデミーのブースで『Symphony Kill Chord』を出展しています。',
			'会場ではぜひ本作を遊んでみてください。本作の紹介は、バンタンゲームアカデミーのTGS2026特設ページでもご覧いただけます。',
		],
		links: [
			{
				label: 'VANTAN GAME ACADEMY TGS2026 特設ページ（Symphony Kill Chord）',
				href: 'https://www.vantan-game.com/special/tgs/sub_page_02.html',
			},
		],
	},
	{
		slug: 'famitsu-2026-09',
		date: '2026-09-19',
		title: 'ファミ通.comで紹介されました',
		body: [
			'ファミ通.comにて、本間ひまわりさん（にじさんじ）がバンタンの学生のゲーム制作をサポートした企画の記事が公開され、『Symphony Kill Chord』が紹介されました。',
			'記事は下記のリンクからご覧いただけます。',
		],
		links: [
			{
				label: 'ファミ通.com の記事',
				href: 'https://www.famitsu.com/article/202609/88578',
			},
		],
	},
];

export const sortedNews = [...news].sort((a, b) => b.date.localeCompare(a.date));

export const newsHref = (item: NewsItem) => `${import.meta.env.BASE_URL}news/${item.slug}`;
