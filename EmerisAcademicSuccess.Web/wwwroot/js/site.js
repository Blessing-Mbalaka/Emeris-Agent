const form = document.getElementById("agent-form");
const messageInput = document.getElementById("agent-message");
const submitButton = document.getElementById("agent-submit");
const resultContainer = document.getElementById("agent-result");

if (form && messageInput && submitButton && resultContainer) {
	form.addEventListener("submit", async (event) => {
		event.preventDefault();

		const message = messageInput.value.trim();
		if (!message) {
			resultContainer.innerHTML = "<p class=\"text-rose-600\">Enter a prompt before running the agent.</p>";
			return;
		}

		submitButton.setAttribute("disabled", "disabled");
		submitButton.classList.add("opacity-60");
		resultContainer.innerHTML = "<p class=\"text-slate-500\">Running planning, tool execution, and response synthesis...</p>";

		try {
			const response = await fetch("/agent/ask", {
				method: "POST",
				headers: {
					"Content-Type": "application/json"
				},
				body: JSON.stringify({ message })
			});

			if (!response.ok) {
				throw new Error("The agent request failed.");
			}

			const payload = await response.json();
			resultContainer.innerHTML = renderAgentResponse(payload);
		} catch (error) {
			resultContainer.innerHTML = `<p class="text-rose-600">${escapeHtml(error.message || "The agent request failed.")}</p>`;
		} finally {
			submitButton.removeAttribute("disabled");
			submitButton.classList.remove("opacity-60");
		}
	});
}

function renderAgentResponse(payload) {
	const actions = Array.isArray(payload.actions)
		? payload.actions.map(action => `<li class="rounded-xl bg-white px-3 py-2"><span class="font-semibold text-slate-900">${escapeHtml(action.toolName)}</span>: ${escapeHtml(action.message)}</li>`).join("")
		: "";

	const citations = Array.isArray(payload.citations)
		? payload.citations.map(citation => `
			<li class="rounded-xl border border-slate-200 bg-white px-3 py-3">
				<p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">${escapeHtml(citation.documentName)} • ${escapeHtml(citation.category)}</p>
				<p class="mt-2 text-sm leading-6 text-slate-600">${escapeHtml(citation.excerpt)}</p>
			</li>`).join("")
		: "";

	return `
		<div class="space-y-4">
			<div>
				<p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-400">Agent answer</p>
				<div class="mt-2 whitespace-pre-line text-sm leading-7 text-slate-700">${escapeHtml(payload.finalResponse || "No response was generated.")}</div>
			</div>
			<div>
				<p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-400">Reasoning summary</p>
				<p class="mt-2 text-sm leading-7 text-slate-600">${escapeHtml(payload.reasoningSummary || "No reasoning summary was returned.")}</p>
			</div>
			<div>
				<p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-400">Tool actions</p>
				<ul class="mt-2 space-y-2 text-sm text-slate-600">${actions || "<li class=\"rounded-xl bg-white px-3 py-2\">No tool actions were recorded.</li>"}</ul>
			</div>
			<div>
				<p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-400">Retrieved context (${escapeHtml(payload.searchMode || "n/a")})</p>
				<ul class="mt-2 space-y-2 text-sm text-slate-600">${citations || "<li class=\"rounded-xl border border-slate-200 bg-white px-3 py-3\">No document citations were returned.</li>"}</ul>
			</div>
		</div>`;
}

function escapeHtml(value) {
	return String(value)
		.replaceAll("&", "&amp;")
		.replaceAll("<", "&lt;")
		.replaceAll(">", "&gt;")
		.replaceAll('"', "&quot;")
		.replaceAll("'", "&#39;");
}
