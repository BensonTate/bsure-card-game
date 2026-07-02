// Vanilla JS frontend for the card game API.
// No framework, no build step. All game logic lives in the API — this file
// is purely rendering and event handling.

const API_BASE = "http://localhost:5155";
const PAGE_SIZE = 10;


const SUIT_SYMBOLS = {
    Diamonds: "♦",
    Hearts: "♥",
    Spades: "♠",
    Clubs: "♣"
};
const RED_SUITS = new Set(["Diamonds", "Hearts"]);

// Short display strings for ranks (K instead of King, etc.) so cards read
// like real playing cards.
const RANK_DISPLAY = {
    Two: "2", Three: "3", Four: "4", Five: "5", Six: "6",
    Seven: "7", Eight: "8", Nine: "9", Ten: "10",
    Jack: "J", Queen: "Q", King: "K", Ace: "A"
};

// State — kept minimal. Only what's needed to enable Re-deal after a Deal
// and to page through past games.
let currentGameId = null;
let pastGamesPage = 1;
let pastGamesTotalPages = 1;

// ---------- API calls ----------

// Central fetch wrapper. Every response is checked for HTTP errors and
// ProblemDetails bodies. Errors bubble up as thrown Errors carrying the
// real API message, so the caller can toast it directly.
async function apiCall(path, options = {}) {
    let response;
    try {
        response = await fetch(`${API_BASE}${path}`, {
            ...options,
            headers: { "Content-Type": "application/json", ...(options.headers || {}) }
        });
    } catch (networkError) {
        // Connection refused, DNS failure, etc. — fetch itself throws.
        throw new Error(`Cannot reach API at ${API_BASE}. Is it running?`);
    }

    if (!response.ok) {
        // Try to read the ProblemDetails body for a real message.
        // Fall back to the status text if the body isn't JSON.
        let message = `Request failed with status ${response.status}`;
        try {
            const problem = await response.json();
            if (problem.detail) message = problem.detail;
            else if (problem.title) message = problem.title;
        } catch { /* body wasn't JSON, use the default message */ }
        throw new Error(message);
    }

    return response.json();
}

const api = {
    createGame: () => apiCall("/games", { method: "POST" }),
    getGame: (id) => apiCall(`/games/${id}`),
    listGames: (page, pageSize) =>
        apiCall(`/games?page=${page}&pageSize=${pageSize}`),
    redealGame: (id) => apiCall(`/games/${id}/redeal`, { method: "POST" })
};

// ---------- Rendering ----------

function renderGame(game) {
    currentGameId = game.id;
    document.getElementById("redeal-btn").disabled = false;

    document.getElementById("game-info").classList.remove("hidden");
    document.getElementById("game-id").textContent = game.id;

    const winners = game.players.filter(p => p.isWinner);
    const winnerSummary = winners.length === 1
        ? `Winner: ${winners[0].name} (${winners[0].handScore})`
        : `Joint winners: ${winners.map(w => w.name).join(", ")}`;
    document.getElementById("winner-summary").textContent = winnerSummary;

    const grid = document.getElementById("players");
    grid.innerHTML = "";
    for (const player of game.players) {
        grid.appendChild(renderPlayer(player));
    }
}

function renderPlayer(player) {
    const el = document.createElement("div");
    el.className = "player" + (player.isWinner ? " winner" : "");

    // Suit product is only shown when it's populated — i.e. this player was
    // in a tie-break. Null means "not applicable" so we omit it entirely.
    const suitProductBlock = player.suitProduct != null
        ? `<span class="suit-product">Suit product: ${player.suitProduct}</span>`
        : "";

    const winnerBadge = player.isWinner
        ? `<span class="winner-badge">Winner</span>` : "";

    el.innerHTML = `
        <div class="player-header">
            <div>
                <span class="player-name">${escape(player.name)}</span>
                ${winnerBadge}
            </div>
            <div class="player-score">
                Hand <strong>${player.handScore}</strong>
                ${suitProductBlock}
            </div>
        </div>
        <div class="cards">
            ${player.cards.map(renderCard).join("")}
        </div>
    `;
    return el;
}

function renderCard(card) {
    const colourClass = RED_SUITS.has(card.suit) ? "red" : "black";
    const rank = RANK_DISPLAY[card.rank] || card.rank;
    const suit = SUIT_SYMBOLS[card.suit] || "?";
    // Small "deck badge" so the two-decks rule is visible in the UI.
    // Cards from deck 2 get a subtle marker so it's clear when duplicates
    // are legitimate rather than a bug.
    const deckMark = card.deckId === 2 ? "•" : "";
    return `
        <div class="card ${colourClass}" title="Deck ${card.deckId}">
            <span class="rank">${rank}${deckMark}</span>
            <span class="suit">${suit}</span>
        </div>
    `;
}

function renderPastGames(paged) {
    pastGamesPage = paged.page;
    pastGamesTotalPages = paged.totalPages;

    const list = document.getElementById("past-games-list");
    list.innerHTML = "";

    if (paged.items.length === 0) {
        list.innerHTML = `<p style="color: var(--muted); padding: 12px;">No games yet.</p>`;
    } else {
        for (const g of paged.items) {
            const row = document.createElement("div");
            row.className = "past-game-row";
            const winners = g.winnerNames.length > 0
                ? g.winnerNames.join(", ")
                : "—";
            row.innerHTML = `
                <span class="id">#${g.id}</span>
                <span class="date">${new Date(g.createdAt + "Z").toLocaleString()}</span>
                <span class="winners">${escape(winners)} (${g.topHandScore})</span>
                <button data-game-id="${g.id}">View</button>
            `;
            row.querySelector("button").addEventListener("click", async () => {
                await loadGame(g.id);
                hidePastGames();
            });
            list.appendChild(row);
        }
    }

    document.getElementById("page-indicator").textContent =
        `Page ${paged.page} of ${Math.max(paged.totalPages, 1)} — ${paged.totalCount} game(s)`;
    document.getElementById("prev-page-btn").disabled = paged.page <= 1;
    document.getElementById("next-page-btn").disabled = paged.page >= paged.totalPages;
}

// ---------- Handlers ----------

async function dealNewGame() {
    try {
        const game = await api.createGame();
        renderGame(game);
    } catch (err) {
        showToast(err.message);
    }
}

async function redealCurrentGame() {
    if (currentGameId == null) {
        showToast("No game to re-deal. Click Deal first.");
        return;
    }
    try {
        const game = await api.redealGame(currentGameId);
        renderGame(game);
    } catch (err) {
        showToast(err.message);
    }
}

async function loadGame(id) {
    try {
        const game = await api.getGame(id);
        renderGame(game);
    } catch (err) {
        showToast(err.message);
    }
}

async function showPastGames(page = 1) {
    try {
        const paged = await api.listGames(page, PAGE_SIZE);
        renderPastGames(paged);
        document.getElementById("past-games-panel").classList.remove("hidden");
    } catch (err) {
        showToast(err.message);
    }
}

function hidePastGames() {
    document.getElementById("past-games-panel").classList.add("hidden");
}

// ---------- Toast ----------

let toastTimer = null;
function showToast(message) {
    const toast = document.getElementById("toast");
    toast.textContent = message;
    toast.classList.remove("hidden");
    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.add("hidden"), 5000);
}

// ---------- Utility ----------

// Defensive HTML escaping. The player names come from the API so this is
// paranoia rather than a real XSS vector today, but it's cheap insurance.
function escape(s) {
    return String(s)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// ---------- Wire up ----------

document.getElementById("deal-btn").addEventListener("click", dealNewGame);
document.getElementById("redeal-btn").addEventListener("click", redealCurrentGame);
document.getElementById("past-games-btn").addEventListener("click", () => showPastGames(1));
document.getElementById("close-past-btn").addEventListener("click", hidePastGames);
document.getElementById("prev-page-btn").addEventListener("click", () => showPastGames(pastGamesPage - 1));
document.getElementById("next-page-btn").addEventListener("click", () => showPastGames(pastGamesPage + 1));