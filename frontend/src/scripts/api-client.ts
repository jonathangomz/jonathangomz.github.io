import type { ContactLink, Job } from '../data/profile';
import { escape, punct, str } from '../utils/json';

interface AboutResponse {
	paragraphs: string[];
	philosophy: string;
}

type StackResponse = Record<string, string[]>;

interface FetchResult<T> {
	data: T;
	ms: number;
	contentType: string | null;
}

interface LogStreamEntry {
	timestamp: string;
	level: string;
	source: string;
	message: string;
}

const DEFAULT_API_URL = 'http://localhost:5080';
const REQUEST_TIMEOUT_MS = 4000;
const HEALTH_POLL_INTERVAL_MS = 15000;
const MAX_LOG_DOM_ENTRIES = 100;

const SOCIAL_ICON_PATHS: Record<string, string> = {
	github:
		'M384 32c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l320 0zM223.7 96c-88.4 0-159.7 72.2-159.7 160.6 0 69.4 44.1 126.9 103.4 148.4 8.4 3.1 16.6-2.5 16.6-10.9l0-25c-4.4 1.9-10 3.1-15 3.1-20.6 0-32.8-11.2-41.6-32.2-3.4-8.4-7.2-13.4-14.4-14.4-3.7-.3-5-1.9-5-3.8 0-3.7 6.2-6.6 12.5-6.6 9.1 0 16.9 5.6 25 17.2 6.2 9.1 12.8 13.1 20.6 13.1s12.8-2.8 20-10c5.3-5.3 9.4-10 13.1-13.1-41.3-5-70.3-34.7-70.3-73.1 0-15.6 5.6-32.5 15-43.8-4.1-10.3-3.4-32.2 1.2-41.2 12.5-1.6 29.4 5 39.4 14.1 11.9-3.7 24.4-5.6 39.7-5.6s27.8 1.9 39.1 5.3c9.7-8.8 26.9-15.3 39.4-13.8 4.4 8.4 5 30.3 .9 40.9 10 11.9 15.3 27.8 15.3 44.1 0 38.4-29.1 67.5-70.9 72.8 10.6 6.9 17.8 21.9 17.8 39.1l0 32.5c0 9.4 7.8 14.7 17.2 10.9 56.6-21.6 100.9-78.1 100.9-148.1 0-88.4-71.9-160.6-160.3-160.6z',
	instagram:
		'M194.4 211.7a53.3 53.3 0 1 0 59.2 88.6 53.3 53.3 0 1 0 -59.2-88.6zm142.3-68.4c-5.2-5.2-11.5-9.3-18.4-12-18.1-7.1-57.6-6.8-83.1-6.5-4.1 0-7.9 .1-11.2 .1s-7.2 0-11.4-.1c-25.5-.3-64.8-.7-82.9 6.5-6.9 2.7-13.1 6.8-18.4 12s-9.3 11.5-12 18.4c-7.1 18.1-6.7 57.7-6.5 83.2 0 4.1 .1 7.9 .1 11.1s0 7-.1 11.1c-.2 25.5-.6 65.1 6.5 83.2 2.7 6.9 6.8 13.1 12 18.4s11.5 9.3 18.4 12c18.1 7.1 57.6 6.8 83.1 6.5 4.1 0 7.9-.1 11.2-.1s7.2 0 11.4 .1c25.5 .3 64.8 .7 82.9-6.5 6.9-2.7 13.1-6.8 18.4-12s9.3-11.5 12-18.4c7.2-18 6.8-57.4 6.5-83 0-4.2-.1-8.1-.1-11.4s0-7.1 .1-11.4c.3-25.5 .7-64.9-6.5-83-2.7-6.9-6.8-13.1-12-18.4l0 .2zm-67.1 44.5c18.1 12.1 30.6 30.9 34.9 52.2s-.2 43.5-12.3 61.6c-6 9-13.7 16.6-22.6 22.6s-19 10.1-29.6 12.2c-21.3 4.2-43.5-.2-61.6-12.3s-30.6-30.9-34.9-52.2 .2-43.5 12.2-61.6 30.9-30.6 52.2-34.9 43.5 .2 61.6 12.2l.1 0zm29.2-1.3c-3.1-2.1-5.6-5.1-7.1-8.6s-1.8-7.3-1.1-11.1 2.6-7.1 5.2-9.8 6.1-4.5 9.8-5.2 7.6-.4 11.1 1.1 6.5 3.9 8.6 7 3.2 6.8 3.2 10.6c0 2.5-.5 5-1.4 7.3s-2.4 4.4-4.1 6.2-3.9 3.2-6.2 4.2-4.8 1.5-7.3 1.5c-3.8 0-7.5-1.1-10.6-3.2l-.1 0zM448 96c0-35.3-28.7-64-64-64L64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l320 0c35.3 0 64-28.7 64-64l0-320zM357 389c-18.7 18.7-41.4 24.6-67 25.9-26.4 1.5-105.6 1.5-132 0-25.6-1.3-48.3-7.2-67-25.9s-24.6-41.4-25.8-67c-1.5-26.4-1.5-105.6 0-132 1.3-25.6 7.1-48.3 25.8-67s41.5-24.6 67-25.8c26.4-1.5 105.6-1.5 132 0 25.6 1.3 48.3 7.1 67 25.8s24.6 41.4 25.8 67c1.5 26.3 1.5 105.4 0 131.9-1.3 25.6-7.1 48.3-25.8 67l0 .1z',
};

function getApiBaseUrl(): string {
	const raw = import.meta.env.PUBLIC_API_URL as string | undefined;
	const base = raw && raw.trim().length > 0 ? raw.trim() : DEFAULT_API_URL;
	return base.replace(/\/+$/, '');
}

function renderSocialIconSvg(name: string): string {
	const path = SOCIAL_ICON_PATHS[name] ?? SOCIAL_ICON_PATHS.github;
	return (
		`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" width="0.875em" height="1em" ` +
		`fill="currentColor" aria-hidden="true" focusable="false" style="display:inline-block;vertical-align:-0.125em">` +
		`<path d="${path}" /></svg>`
	);
}

function setApiStatus(status: 'online' | 'offline'): void {
	const statusEl = document.getElementById('api-status');
	if (statusEl) {
		statusEl.setAttribute('data-api-status', status);
		const textEl = statusEl.querySelector<HTMLElement>('[data-api-status-text]');
		if (textEl) {
			textEl.textContent = status;
		}
	}
}

function setRouteLatency(path: string, ms: number): void {
	const msEl = document.querySelector<HTMLElement>(`[data-route-ms="${path}"]`);
	if (msEl) {
		msEl.textContent = `${Math.max(1, Math.round(ms))}ms`;
	}
}

function setRouteContentType(path: string, contentTypeHeader: string | null): void {
	if (!contentTypeHeader) return;
	const mediaType = contentTypeHeader.split(';')[0]?.trim();
	if (!mediaType) return;
	const ctEl = document.querySelector<HTMLElement>(`[data-route-ct="${path}"]`);
	if (ctEl) {
		ctEl.textContent = mediaType;
	}
}

function setRouteNote(path: string, note: string): void {
	const noteEl = document.querySelector<HTMLElement>(`[data-route-note="${path}"]`);
	if (noteEl) {
		noteEl.textContent = note;
	}
}

function setRouteBodyHtml(path: string, html: string): void {
	const bodyEl = document.querySelector<HTMLElement>(`[data-route-body="${path}"]`);
	if (bodyEl) {
		bodyEl.innerHTML = html;
	}
}

function parseAboutMarkdown(markdown: string): AboutResponse {
	const blocks = markdown
		.split(/\r?\n\s*\r?\n/)
		.map((block) => block.trim())
		.filter((block) => block.length > 0);

	const paragraphs: string[] = [];
	let philosophy = '';

	for (const block of blocks) {
		if (block.startsWith('>')) {
			philosophy = block.replace(/^>\s*/, '').trim();
		} else {
			paragraphs.push(block);
		}
	}

	return { paragraphs, philosophy };
}

function parseExperienceMarkdown(markdown: string): Job[] {
	const sections = markdown
		.split(/^###\s+/m)
		.map((s) => s.trim())
		.filter((s) => s.length > 0);

	return sections.map((section) => {
		const lines = section
			.split(/\r?\n/)
			.map((line) => line.trim())
			.filter((line) => line.length > 0);

		const company = lines[0] ?? '';
		const metaLine = lines[1] ?? '';
		const [role = '', period = '', location = ''] = metaLine.split('·').map((part) => part.trim());
		const highlights = lines
			.slice(2)
			.filter((line) => line.startsWith('- '))
			.map((line) => line.slice(2).trim());

		return { company, role, period, location, highlights };
	});
}

function renderAbout(about: AboutResponse): string {
	const paragraphs = about.paragraphs.map((text) => `<p>${escape(text)}</p>`).join('');
	return `<div class="doc">${paragraphs}<blockquote>${escape(about.philosophy)}</blockquote></div>`;
}

function renderExperience(jobs: Job[]): string {
	const items = jobs
		.map((job) => {
			const highlights = job.highlights.map((h) => `<li>${escape(h)}</li>`).join('');
			return (
				`<div class="job">` +
				`<h3>${escape(job.company)}</h3>` +
				`<span class="period">${escape(job.role)} · ${escape(job.period)} · ${escape(job.location)}</span>` +
				`<ul>${highlights}</ul>` +
				`</div>`
			);
		})
		.join('');
	return `<div class="doc">${items}</div>`;
}

function renderStack(stack: StackResponse): string {
	const entries = Object.entries(stack);
	const rows = entries
		.map(([key, value], index) => {
			const rendered = Array.isArray(value)
				? punct('[') + value.map(str).join(punct(',') + ' ') + punct(']')
				: str(String(value));
			const comma = index < entries.length - 1 ? punct(',') : '';
			return (
				`<span class="kc"><span class="k">"${escape(key)}"</span>${punct(':')}</span>` +
				`<span class="v">${rendered}${comma}</span>`
			);
		})
		.join('');

	return `<div class="json"><span class="p">&#123;</span><div class="json-rows">${rows}</div><span class="p">&#125;</span></div>`;
}

function renderContact(links: ContactLink[]): string {
	const rows = links
		.map((link, index) => {
			const anchor =
				`<a class="s" href="${escape(link.href)}" target="_blank" rel="noreferrer">` +
				`"${escape(link.display)}"</a>`;
			const comma = index < links.length - 1 ? punct(',') : '';
			return (
				`<span class="kc"><span class="k">"${escape(link.key)}"</span>${punct(':')}</span>` +
				`<span class="v">${anchor}${comma}</span>`
			);
		})
		.join('');

	return `<div class="json json-links"><span class="p">&#123;</span><div class="json-rows">${rows}</div><span class="p">&#125;</span></div>`;
}

function updateFooterLinks(links: ContactLink[]): void {
	const footerEl = document.querySelector<HTMLElement>('[data-footer-links]');
	if (!footerEl) return;

	footerEl.innerHTML = links
		.map(
			(link) =>
				`<a target="_blank" href="${escape(link.href)}" rel="noreferrer" aria-label="${escape(link.key)}">` +
				`${renderSocialIconSvg(link.icon)}` +
				`</a>`,
		)
		.join('');
}

async function fetchJsonWithTiming<T>(baseUrl: string, path: string): Promise<FetchResult<T>> {
	const startedAt = performance.now();
	const response = await fetch(`${baseUrl}${path}`, {
		headers: { Accept: 'application/json' },
		signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS),
	});

	if (!response.ok) {
		throw new Error(`GET ${path} returned ${response.status}`);
	}

	const data = (await response.json()) as T;
	const ms = performance.now() - startedAt;
	return { data, ms, contentType: response.headers.get('content-type') };
}

async function fetchTextWithTiming(baseUrl: string, path: string, accept: string): Promise<FetchResult<string>> {
	const startedAt = performance.now();
	const response = await fetch(`${baseUrl}${path}`, {
		headers: { Accept: accept },
		signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS),
	});

	if (!response.ok) {
		throw new Error(`GET ${path} returned ${response.status}`);
	}

	const data = await response.text();
	const ms = performance.now() - startedAt;
	return { data, ms, contentType: response.headers.get('content-type') };
}

async function checkHealth(baseUrl: string): Promise<boolean> {
	try {
		const response = await fetch(`${baseUrl}/health`, {
			signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS),
		});
		return response.ok;
	} catch {
		return false;
	}
}

async function hydrateRoutes(baseUrl: string): Promise<boolean> {
	const results = await Promise.allSettled([
		fetchTextWithTiming(baseUrl, '/about', 'text/markdown').then(({ data, ms, contentType }) => {
			setRouteBodyHtml('/about', renderAbout(parseAboutMarkdown(data)));
			setRouteContentType('/about', contentType);
			setRouteLatency('/about', ms);
		}),
		fetchTextWithTiming(baseUrl, '/experience', 'text/markdown').then(({ data, ms, contentType }) => {
			setRouteBodyHtml('/experience', renderExperience(parseExperienceMarkdown(data)));
			setRouteContentType('/experience', contentType);
			setRouteLatency('/experience', ms);
		}),
		fetchJsonWithTiming<StackResponse>(baseUrl, '/stack').then(({ data, ms, contentType }) => {
			setRouteBodyHtml('/stack', renderStack(data));
			setRouteContentType('/stack', contentType);
			setRouteLatency('/stack', ms);
		}),
		fetchTextWithTiming(baseUrl, '/projects', 'text/html').then(({ data, ms, contentType }) => {
			setRouteBodyHtml('/projects', data);
			const cardCount = document.querySelectorAll('[data-route-body="/projects"] .card').length;
			if (cardCount > 0) {
				setRouteNote('/projects', `${cardCount} items`);
			}
			setRouteContentType('/projects', contentType);
			setRouteLatency('/projects', ms);
		}),
		fetchJsonWithTiming<ContactLink[]>(baseUrl, '/contact').then(({ data, ms, contentType }) => {
			setRouteBodyHtml('/contact', renderContact(data));
			setRouteNote('/contact', `${data.length} links`);
			setRouteContentType('/contact', contentType);
			setRouteLatency('/contact', ms);
			updateFooterLinks(data);
		}),
	]);

	return results.every((result) => result.status === 'fulfilled');
}

export async function initPortfolioApiClient(): Promise<void> {
	const baseUrl = getApiBaseUrl();
	let isOnline = false;
	let logEventSource: EventSource | null = null;
	let totalLogEntries = 0;

	const drawerEl = document.querySelector<HTMLElement>('[data-log-drawer]');
	const toggleBtn = document.querySelector<HTMLButtonElement>('[data-log-toggle]');
	const consoleEl = document.querySelector<HTMLElement>('[data-log-console]');
	const stateEl = document.querySelector<HTMLElement>('[data-log-state]');
	const countEl = document.querySelector<HTMLElement>('[data-log-count]');
	const emptyEl = document.querySelector<HTMLElement>('[data-log-empty]');

	function setLogStreamStatus(status: 'live' | 'offline'): void {
		if (drawerEl) {
			drawerEl.setAttribute('data-stream-status', status);
		}
		if (stateEl) {
			stateEl.textContent = status;
		}
	}

	function formatTimestamp(isoString: string): string {
		const date = new Date(isoString);
		if (Number.isNaN(date.getTime())) {
			return isoString;
		}
		const hh = String(date.getHours()).padStart(2, '0');
		const mm = String(date.getMinutes()).padStart(2, '0');
		const ss = String(date.getSeconds()).padStart(2, '0');
		const ms = String(date.getMilliseconds()).padStart(3, '0');
		return `${hh}:${mm}:${ss}.${ms}`;
	}

	function appendLogEntry(entry: LogStreamEntry): void {
		if (!consoleEl) return;

		if (emptyEl && !emptyEl.hidden) {
			emptyEl.hidden = true;
		}

		const lineEl = document.createElement('p');
		lineEl.className = 'log-line';
		lineEl.setAttribute('data-source', entry.source);

		const tsSpan = document.createElement('span');
		tsSpan.className = 'log-ts';
		tsSpan.textContent = `[${formatTimestamp(entry.timestamp)}] `;

		const lvlSpan = document.createElement('span');
		lvlSpan.className = 'log-lvl';
		lvlSpan.setAttribute('data-level', entry.level);
		lvlSpan.textContent = `${entry.level} `;

		const msgSpan = document.createElement('span');
		msgSpan.className = 'log-msg';
		msgSpan.textContent = entry.message;

		lineEl.append(tsSpan, lvlSpan, msgSpan);
		consoleEl.appendChild(lineEl);

		const lines = consoleEl.querySelectorAll('.log-line');
		if (lines.length > MAX_LOG_DOM_ENTRIES) {
			lines[0]?.remove();
		}

		totalLogEntries += 1;
		if (countEl) {
			countEl.textContent = String(totalLogEntries);
		}

		consoleEl.scrollTop = consoleEl.scrollHeight;
	}

	function connectLogStream(): void {
		if (logEventSource || typeof EventSource === 'undefined') {
			return;
		}

		const source = new EventSource(`${baseUrl}/logs/stream`);
		logEventSource = source;

		source.onopen = () => {
			setLogStreamStatus('live');
		};

		source.onmessage = (event: MessageEvent<string>) => {
			try {
				const parsed = JSON.parse(event.data) as LogStreamEntry;
				if (parsed && typeof parsed.message === 'string') {
					setLogStreamStatus('live');
					appendLogEntry(parsed);
				}
			} catch {
				// Ignore malformed frames
			}
		};

		source.onerror = () => {
			if (!isOnline) {
				disconnectLogStream();
			}
		};
	}

	function disconnectLogStream(): void {
		if (logEventSource) {
			logEventSource.close();
			logEventSource = null;
		}
		setLogStreamStatus('offline');
	}

	if (toggleBtn && consoleEl) {
		toggleBtn.addEventListener('click', () => {
			const expanded = toggleBtn.getAttribute('aria-expanded') === 'true';
			const nextExpanded = !expanded;
			toggleBtn.setAttribute('aria-expanded', String(nextExpanded));
			consoleEl.hidden = !nextExpanded;
			if (nextExpanded) {
				consoleEl.scrollTop = consoleEl.scrollHeight;
			}
		});
	}

	async function syncWithApi(forceHydrate: boolean): Promise<void> {
		const healthy = await checkHealth(baseUrl);
		if (!healthy) {
			isOnline = false;
			setApiStatus('offline');
			disconnectLogStream();
			return;
		}

		connectLogStream();

		if (forceHydrate || !isOnline) {
			const hydrated = await hydrateRoutes(baseUrl);
			isOnline = hydrated;
			setApiStatus(hydrated ? 'online' : 'offline');
			if (!hydrated) {
				disconnectLogStream();
			}
			return;
		}

		setApiStatus('online');
	}

	await syncWithApi(true);

	window.setInterval(() => {
		void syncWithApi(false);
	}, HEALTH_POLL_INTERVAL_MS);
}
