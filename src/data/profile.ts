export interface Job {
	company: string;
	role: string;
	period: string;
	location: string;
	highlights: string[];
}

/** GET /about — prose, so it renders as prose. */
export const about = {
	paragraphs: [
		'Senior software developer working on production systems since 2016, specialising in backend. Strong background in designing RESTful and gRPC APIs, worker services and database architecture, refactoring legacy systems into modern, maintainable ones.',
		'Experienced in academic and financial environments, collaborating with multidisciplinary teams and delivering scalable backend services.',
		'Not a frontend specialist, but has delivered enough of it to lead those decisions, and to let the way a client actually consumes an API shape how that API is designed.',
	],
	philosophy:
		"Write your code so well that you are replaceable, but write your code so well that they don't want to replace you.",
};

/** GET /experience — headings and bullet lists. */
export const experience: Job[] = [
	{
		company: 'One Card',
		role: 'Software Developer',
		period: 'Dec 2021 — present',
		location: 'Monterrey, Nuevo León',
		highlights: [
			'Built a transaction authorizer connecting to PROSA over the ISO 8583 standard.',
			'Built a TUI to orchestrate multiple authorizer instances across ports over gRPC, keeping latency low between services.',
			'Published a Clean Architecture project template to Azure Artifacts, standardising rate limiting, unit tests, OpenTelemetry and health checks across company projects.',
			'Published NuGet packages for shared logging, notifications, database connections, encryption, storage and error handling.',
			'Built an API gateway with Ocelot, improving analytics and opening the path to microservices.',
			'Led the refactor of a Single Sign-On API to centralise authentication logic.',
			'Led the migration of the API documentation system from .NET Framework 4.7.2 to .NET 10, adding OpenAPI support.',
			'Held overall test coverage at 70% with FluentAssertions, enforced by Azure Pipelines branch policies.',
		],
	},
	{
		company: 'Montemorelos University',
		role: 'Full Stack Developer & Teacher',
		period: 'Dec 2016 — Dec 2021',
		location: 'Montemorelos, Nuevo León',
		highlights: [
			"Led the university's open-source mobile app: Angular, later migrated to Flutter, with a NestJS API.",
			'Published open-source Dart packages to pub.dev (ummobile_sdk, ummobile_custom_http) to decouple business logic.',
			'Sole maintainer of EduAdvent, a school management system deployed across Mexico, El Salvador and Panama: J2EE codebase, PostgreSQL database and Tomcat production deploys.',
			'Configured Linux servers with Nginx for sandbox and production environments.',
			'Taught web and software development at the Faculty of Engineering and Technology.',
		],
	},
];

/**
 * GET /stack — flat lists of short strings. This is the one place where the
 * data genuinely is JSON-shaped, so it is served as JSON.
 */
export const stack: Record<string, string[]> = {
	languages: ['C#', 'T-SQL', 'PL/pgSQL', 'TypeScript', 'JavaScript', 'Dart', 'Java'],
	backend: ['ASP.NET Core 8+', '.NET Framework 4.7.2', 'NestJS', 'Express', 'Spring Boot'],
	frontend: ['Flutter', 'Angular', 'React', 'Vue'],
	databases: ['SQL Server', 'PostgreSQL', 'MongoDB'],
	apis: ['REST', 'gRPC', 'OpenAPI'],
	payments: ['ISO 8583'],
	ops: ['Azure DevOps', 'GitHub Actions', 'Nginx', 'IIS'],
};

export interface ContactLink {
	/** JSON key, and the footer's accessible name. */
	key: string;
	/** What the JSON value reads as — the URL without its scheme. */
	display: string;
	href: string;
	icon: 'github' | 'instagram';
}

/**
 * GET /contact — a small flat object, so it is served as JSON.
 *
 * The footer renders from this same list, so the two places these links appear
 * cannot drift apart: adding one here adds it to both.
 */
export const contact: ContactLink[] = [
	{
		key: 'github',
		display: 'github.com/jonathangomz',
		href: 'https://github.com/jonathangomz',
		icon: 'github',
	},
	{
		key: 'instagram',
		display: 'instagram.com/jonathan_gomz',
		href: 'https://instagram.com/jonathan_gomz',
		icon: 'instagram',
	},
];
