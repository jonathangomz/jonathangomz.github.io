export interface Project {
	title: string;
	details: string;
	/** Public page for the project. Omitted when the repository is the only destination. */
	href?: string;
	/** Label for the `href` button, when "Go to the page" is not what it leads to. */
	hrefLabel?: string;
	repo: string;
}

export const projects: Project[] = [
	{
		title: 'notion_api',
		details:
			'A Dart client wrapping the Notion public API, published to pub.dev across 14 releases.',
		href: 'https://pub.dev/packages/notion_api',
		hrefLabel: 'Go to pub.dev',
		repo: 'https://github.com/jonathangomz/notion_api',
	},
	{
		title: 'UMMobile app',
		details:
			"Montemorelos University's open source mobile app, built with Flutter after migrating it from Angular.",
		repo: 'https://github.com/UMMobile/ummobile_app',
	},
	{
		title: 'UMMobile API',
		details:
			'The NestJS API behind the app, collecting every other API the university exposes behind one surface.',
		repo: 'https://github.com/UMMobile/ummobile-api',
	},
	{
		title: 'ummobile_sdk',
		details:
			'A published Dart package wrapping the UMMobile API, so the app does not carry the business logic.',
		href: 'https://pub.dev/packages/ummobile_sdk',
		hrefLabel: 'Go to pub.dev',
		repo: 'https://github.com/UMMobile/ummobile_sdk',
	},
	{
		title: 'ummobile_custom_http',
		details: 'A published Dart package with the HTTP client the rest of the UMMobile packages are built on.',
		href: 'https://pub.dev/packages/ummobile_custom_http',
		hrefLabel: 'Go to pub.dev',
		repo: 'https://github.com/UMMobile/ummobile_custom_http',
	},
	{
		title: 'Daily quote',
		details: 'A simple page that display a different quote every day.',
		href: 'https://jonathangomz.github.io/quotes/',
		repo: 'https://github.com/jonathangomz/quotes',
	},
	{
		title: 'Truth table',
		details: 'A generator of table of truth with a certain condition given.',
		href: 'https://jonathangomz.github.io/TruthTableGenerator/',
		repo: 'https://github.com/jonathangomz/TruthTableGenerator',
	},
	{
		title: 'Devotionals',
		details: 'A simple web application just to read asd devotionals.',
		href: 'https://devotionals.vercel.app/',
		repo: 'https://github.com/jonathangomz/devotionals-app-web',
	},
	{
		title: 'Playlist cloner',
		details:
			'Implements the Spotify OAuth authorization code flow from scratch, then clones a playlist into the signed-in account.',
		href: 'https://playlist-cloner.vercel.app/cloner',
		repo: 'https://github.com/jonathangomz/playlist-cloner',
	},
];
