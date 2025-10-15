/* =======================================
   Script do Chatbot
   ======================================= */
document.addEventListener("DOMContentLoaded", () => {
    // -----------------------------
    // Elementos do DOM
    // -----------------------------
    const chatForm = document.getElementById("chat-form");         // Formulário do chat
    const sendBtn = document.getElementById("send-btn");           // Botão de enviar
    const userInput = document.getElementById("user-input");       // Input do usuário
    const chatMessages = document.getElementById("chat-messages"); // Contêiner de mensagens
    const suggestionButtonsContainer = document.getElementById("suggestion-buttons"); // Contêiner de botões de sugestão

    // -----------------------------
    // Sugestões iniciais
    // -----------------------------
    const suggestions = [
        "Como criar usuário?",
        "Onde vejo os relatórios?",
        "Como alterar minha senha?",
        "O que é um KPI?"
    ];

    // -----------------------------
    // Função para normalizar texto
    // -----------------------------
    function normalizeText(text) {
        return text.toLowerCase()
            .normalize("NFD")                     // Separar caracteres acentuados
            .replace(/[\u0300-\u036f]/g, "")      // Remove acentos
            .replace(/[^\w\s]/g, "")              // Remove pontuação
            .replace(/\s+/g, " ").trim();         // Remove espaços extras
    }

    // -----------------------------
    // Função para adicionar mensagem ao chat
    // -----------------------------
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

        // Rola para o final da conversa
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // -----------------------------
    // Renderiza os botões de sugestão
    // -----------------------------
    function renderSuggestions() {
        suggestionButtonsContainer.innerHTML = '';
        suggestions.forEach(text => {
            const button = document.createElement("button");
            button.className = "suggestion-btn";
            button.innerText = text;

            // Ao clicar no botão, preenche input e envia
            button.addEventListener("click", () => {
                userInput.value = text;
                sendBtn.click();
            });

            suggestionButtonsContainer.appendChild(button);
        });
    }

    // -----------------------------
    // Mensagem inicial do bot com sugestões
    // -----------------------------
    setTimeout(() => {
        addMessage("bot", "Olá! Eu sou a Inteligência Artificial da InovaTech. Posso te ajudar com dúvidas sobre usuários, relatórios, chamados e muito mais.");
        renderSuggestions();
    }, 500);

    // -----------------------------
    // Envio de mensagens
    // -----------------------------
    chatForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;

        // Adiciona mensagem do usuário
        addMessage("user", message);
        userInput.value = "";

        // Remove sugestões temporariamente
        suggestionButtonsContainer.innerHTML = '';

        // Adiciona indicador de "digitando..." do bot
        const typingIndicator = document.createElement("div");
        typingIndicator.className = "chat-message bot typing-indicator";
        chatMessages.appendChild(typingIndicator);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        // Chamada à API do backend
        fetch('/Chatbot/Ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Question: message })
        })
        .then(res => res.json())
        .then(data => {
            typingIndicator.remove(); // remove indicador
            addMessage("bot", data.answer || "Desculpe, não consegui encontrar uma resposta.");
        })
        .catch(err => {
            typingIndicator.remove();
            addMessage("bot", "Ocorreu um erro ao tentar responder. Tente novamente.");
            console.error(err);
        });
    });
});
