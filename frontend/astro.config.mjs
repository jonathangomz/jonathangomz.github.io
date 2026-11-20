// @ts-check
import { defineConfig, fontProviders } from 'astro/config';

// https://astro.build/config
export default defineConfig({
	site: 'https://jonathangomz.github.io',
	fonts: [
		{
			// Self-hosted: the .woff2 lives in src/assets/fonts and is served from
			// this domain, so the site makes no third-party font requests.
			provider: fontProviders.local(),
			name: 'JetBrains Mono',
			cssVariable: '--font-jetbrains-mono',
			fallbacks: ['monospace'],
			options: {
				variants: [
					{
						src: ['./src/assets/fonts/JetBrainsMono-Regular.woff2'],
						weight: 400,
						style: 'normal',
					},
				],
			},
		},
	],
});
