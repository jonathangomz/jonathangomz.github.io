export interface Project {
	title: string;
	details: string;
	href: string;
}

export const projects: Project[] = [
	{
		title: 'Daily quote',
		details: 'A simple page that display a different quote every day.',
		href: 'https://jonathangomz.github.io/quotes/',
	},
	{
		title: 'Truth table',
		details: 'A generator of table of truth with a certain condition given.',
		href: 'https://jonathangomz.github.io/TruthTableGenerator/',
	},
	{
		title: 'Devotionals',
		details: 'A simple web application just to read asd devotionals.',
		href: 'https://devotionals.vercel.app/',
	},
	{
		title: 'Playlist cloner',
		details: 'A webapp made with Nextjs for Spotify playlist cloning using the Spotify api.',
		href: 'https://playlist-cloner.vercel.app/cloner',
	},
];
