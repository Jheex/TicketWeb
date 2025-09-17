// chatbot.js

document.addEventListener("DOMContentLoaded", () => {
    const chatForm = document.getElementById("chat-form");
    const chatInput = document.getElementById("chat-input");
    const chatBox = document.getElementById("chat-box");

    // Função para adicionar mensagem no chat
    const addMessage = (message, sender) => {
        const msgDiv = document.createElement("div");
        msgDiv.classList.add("message", sender);
        msgDiv.textContent = message;
        chatBox.appendChild(msgDiv);
        chatBox.scrollTop = chatBox.scrollHeight;
    };

    // Função para gerar resposta do bot
    const getBotResponse = (userMessage) => {
        userMessage = userMessage.toLowerCase();

        if (userMessage.includes("olá") || userMessage.includes("oi")) {
            return "Olá! Como posso te ajudar?";
        } else if (userMessage.includes("horas")) {
            return `Agora são ${new Date().toLocaleTimeString()}`;
        } else {
            return "Desculpe, não entendi. Pode reformular?";
        }
    };

    // Evento de envio do formulário
    chatForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const userMessage = chatInput.value.trim();
        if (!userMessage) return;

        addMessage(userMessage, "user");
        chatInput.value = "";

        const botMessage = getBotResponse(userMessage);
        setTimeout(() => addMessage(botMessage, "bot"), 500);
    });
});
