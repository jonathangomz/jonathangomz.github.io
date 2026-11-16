export const escape = (value: string) =>
	value
		.replace(/&/g, '&amp;')
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;')
		.replace(/"/g, '&quot;');

export const punct = (value: string) => `<span class="p">${value}</span>`;
export const str = (value: string) => `<span class="s">"${escape(value)}"</span>`;
