// @ts-check
import { defineConfig } from 'astro/config';

const base = '/SymphonyKillChord/';

// https://astro.build/config
export default defineConfig({
	site: 'https://hibiki5201.github.io',
	base,
	redirects: {
		'/': `${base}home`,
	},
});
