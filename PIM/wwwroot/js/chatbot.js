document.addEventListener("DOMContentLoaded", () => {
    const chatForm = document.getElementById("chat-form");
    const sendBtn = document.getElementById("send-btn");
    const userInput = document.getElementById("user-input");
    const chatMessages = document.getElementById("chat-messages");
    const suggestionButtonsContainer = document.getElementById("suggestion-buttons");

    const suggestions = [
        "Como criar usuário?",
        "Onde vejo os relatórios?",
        "Como alterar minha senha?",
        "O que é um KPI?"
    ];

    function normalizeText(text) {
        return text.toLowerCase()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "") // Remove acentos
            .replace(/[^\w\s]/g, "")          // Remove pontuação
            .replace(/\s+/g, " ").trim();     // Remove espaços extras
    }

    function addMessage(sender, text) {
        const now = new Date();
        const formattedTime = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const formattedDate = now.toLocaleDateString('pt-BR');

        const msg = document.createElement("div");
        msg.className = `chat-message ${sender}`;

        const messageText = document.createElement("span");
        messageText.innerHTML = text;

        const messageInfo = document.createElement("span");
        messageInfo.className = "chat-message-info";
        messageInfo.innerText = `${formattedTime} · ${formattedDate}`;

        msg.appendChild(messageText);
        msg.appendChild(messageInfo);
        chatMessages.appendChild(msg);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function renderSuggestions() {
        suggestionButtonsContainer.innerHTML = '';
        suggestions.forEach(text => {
            const button = document.createElement("button");
            button.className = "suggestion-btn";
            button.innerText = text;
            button.addEventListener("click", () => {
                userInput.value = text;
                sendBtn.click();
            });
            suggestionButtonsContainer.appendChild(button);
        });
    }

    setTimeout(() => {
        addMessage("bot", "Olá! Eu sou a Inteligência Artificial da InovaTech. Posso te ajudar com dúvidas sobre usuários, relatórios, chamados e muito mais.");
        renderSuggestions();
    }, 500);

    chatForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;

        addMessage("user", message);
        userInput.value = "";
        suggestionButtonsContainer.innerHTML = '';

        const typingIndicator = document.createElement("div");
        typingIndicator.className = "chat-message bot typing-indicator";
        typingIndicator.innerText = "Digitando...";
        chatMessages.appendChild(typingIndicator);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        fetch('/Chatbot/Ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Question: message })
        })
        .then(res => res.json())
        .then(data => {
            typingIndicator.remove();
            addMessage("bot", data.answer || "Desculpe, não consegui encontrar uma resposta.");
        })
        .catch(err => {
            typingIndicator.remove();
            addMessage("bot", "Ocorreu um erro ao tentar responder. Tente novamente.");
            console.error(err);
        });
    });
});
