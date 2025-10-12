// Variável de estado para controlar a visualização
let isMyTicketsView = false;

// Variável global (CURRENT_USER_ID) deve ser definida na sua View Razor (.cshtml)
// Exemplo: const CURRENT_USER_ID = "123";

// ===== Função principal para carregar tickets do backend =====
async function loadTickets() {
    let tickets;
    let response;

    // Decide qual endpoint da API chamar com base na variável de estado
    if (isMyTicketsView) {
        response = await fetch('/api/ticketsapi/MyTickets');
    } else {
        response = await fetch('/api/ticketsapi');
    }

    if (!response.ok) {
        if (response.status === 401) {
            alert("Você precisa estar logado para ver seus tickets.");
        } else {
            alert("Erro ao carregar os tickets.");
        }
        return;
    }

    tickets = await response.json();

    // A lógica de filtragem continua a mesma, mas é aplicada após a busca correta
    const searchTerm = document.getElementById("searchInput").value.toLowerCase();
    const statusFilter = document.getElementById("statusFilter").value;
    const priorityFilter = document.getElementById("priorityFilter").value;
    const dateFilter = document.getElementById("dateFilter").value;

    const filteredTickets = tickets.filter(ticket => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchTerm)
            || (ticket.assignedTo ? ticket.assignedTo.toLowerCase().includes(searchTerm) : false) || (ticket.solicitante ? ticket.solicitante.toLowerCase().includes(searchTerm) : false);

        const matchesStatus = statusFilter === "" || ticket.status === statusFilter;
        const matchesPriority = priorityFilter === "" || ticket.priority === priorityFilter;
        
        // Lógica de filtro de 7 dias
        let matchesDate = true;
        if (dateFilter === "last7days") {
            const sevenDaysAgo = Date.now() - 7 * 24 * 60 * 60 * 1000;
            matchesDate = new Date(ticket.dataAbertura) >= new Date(sevenDaysAgo);
        } else if (dateFilter !== "all") {
             // Se tiver outras opções de filtro de data aqui, adicione a lógica
        }

        return matchesSearch && matchesStatus && matchesPriority && matchesDate;
    });

    renderTickets(filteredTickets);
    
    // Mostra a mensagem de feedback se algum filtro estiver ativo
    const feedbackDiv = document.getElementById('filterFeedback');
    
    // Adicionei uma verificação para a data, já que "Todo Período" é o padrão
    if (searchTerm || statusFilter || priorityFilter || dateFilter !== 'all' || isMyTicketsView) {
        feedbackDiv.style.display = 'flex';
        setTimeout(() => {
            feedbackDiv.style.display = 'none';
        }, 3000); // Esconde a mensagem depois de 3 segundos
    } else {
        feedbackDiv.style.display = 'none';
    }
}

// ===== Renderiza tickets na tela (LÓGICA DOS BOTÕES ATUALIZADA) =====
function renderTickets(tickets) {
    const container = document.getElementById("ticketContainer");
    container.innerHTML = "";

    if (tickets.length === 0) {
        container.innerHTML = `<div class="no-tickets-message">Nenhum ticket encontrado.</div>`;
        return;
    }

    tickets.sort((a, b) => b.id - a.id).forEach(ticket => {
        const priorityClass = {
            "Urgente": "badge-priority-urgente",
            "Alta": "badge-priority-alta",
            "Média": "badge-priority-media",
            "Baixa": "badge-priority-baixa"
        }[ticket.priority] || "badge-priority-baixa";

        const statusClass = {
            "Aberto": "badge-status-aberto",
            "Em Andamento": "badge-status-emandamento",
            "Concluído": "badge-status-concluido",
        }[ticket.status] || "badge-status-aberto";

        const card = document.createElement("div");
        card.className = "ticket-card";

        const statusBorderClass = {
            "Aberto": "status-border-aberto",
            "Em Andamento": "status-border-emandamento",
            "Concluído": "status-border-concluido",
        }[ticket.status] || "";

        card.classList.add(statusBorderClass);

        // Botões de ação (LÓGICA ATUALIZADA AQUI)
        let actionButtonsHtml = '';
        
        // 1. Regra para ATENDER: Status é 'Aberto' E não está atribuído a NINGUÉM
        if (ticket.status === 'Aberto' && !ticket.assignedToId) {
            actionButtonsHtml = `
                <button class="ticket-btn ticket-btn-approve" onclick="approveTicket(${ticket.id})">Atender</button>
            `;
        } 
        // 2. Regra para CONCLUIR: Status é 'Em Andamento' E está atribuído ao USUÁRIO LOGADO
        else if (ticket.status === 'Em Andamento') {
            // Compara o ID do ticket (string) com o ID global (string)
            if (ticket.assignedToId && ticket.assignedToId.toString() === CURRENT_USER_ID.toString()) {
                actionButtonsHtml = `
                    <button class="ticket-btn ticket-btn-concluir" onclick="concludeTicket(${ticket.id})">Concluir</button>
                `;
            }
        }

        const dataAbertura = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'});

        // **Todo o HTML do card dentro de card.innerHTML**
        card.innerHTML = `
        <div class="card-content">
            <div class="card-header">
                <div class="card-title">#${ticket.id} - ${ticket.title}</div>
                <button class="delete-btn" onclick="deleteTicket(${ticket.id})">🗑️</button>
            </div>
            <div><strong>Solicitante:</strong> ${ticket.solicitante || "Não informado"}</div>
            <div><strong>Analista:</strong> ${ticket.assignedTo || "Não atribuído"}</div>
            <div class="ticket-category"><strong>Categoria:</strong> ${ticket.category}</div>
            <div class="ticket-date"><strong>Solicitado em:</strong> ${dataAbertura}</div>
            <div><strong>Atribuído em:</strong> ${ticket.dataAtribuicao 
                ? new Date(ticket.dataAtribuicao).toLocaleString('pt-BR', {year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'}) 
                : "Não atribuído"}</div>
            <div class="ticket-badges-container">
                <span class="ticket-badge ${priorityClass}">Prioridade: ${ticket.priority}</span>
                <span class="ticket-badge ${statusClass}">Status: ${ticket.status}</span>
            </div>
        </div>
        <div class="ticket-actions">
            ${actionButtonsHtml}
            <button class="ticket-btn ticket-btn-details" onclick="viewDetails(${ticket.id})">Detalhes</button>
        </div>`;
        
        document.getElementById("ticketContainer").appendChild(card);
    });

}

// ===== Função de Toast =====
function showToast(message, type = "success") {
    const toast = document.getElementById("toast");
    toast.innerText = message;

    if (type === "success") {
        toast.style.backgroundColor = "#2ecc71"; // verde
    } else if (type === "error") {
        toast.style.backgroundColor = "#e74c3c"; // vermelho
    }

    toast.className = "toast show";

    setTimeout(() => {
        toast.className = toast.className.replace("show", "");
    }, 3000);
}

// ===== Ações =====
async function approveTicket(id) {
    try {
        const response = await fetch(`/api/ticketsapi/approve/${id}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            showToast("Ticket atribuído com sucesso!", "success");
            await loadTickets();
            const modal = document.getElementById("ticketModal");
            // Se o modal estiver aberto, atualiza a view para refletir a nova atribuição
            if (modal.style.display === "flex") viewDetails(id); 
        } else {
            // Captura o erro, como "Ticket já está em andamento"
            const errorText = await response.text(); 
            showToast(errorText || "Erro ao atribuir o ticket.", "error");
        }
    } catch (error) {
        console.error(error);
        showToast("Erro ao tentar atribuir o ticket.", "error");
    }
}


async function concludeTicket(id) {
    try {
        const response = await fetch(`/api/ticketsapi/conclude/${id}`, { method: 'POST' });

        if (response.ok) {
            showToast("Ticket concluído com sucesso!", "success");
            await loadTickets();
            const modal = document.getElementById("ticketModal");
            if (modal.style.display === "flex") viewDetails(id);
        } else {
            const data = await response.json().catch(() => null);
            showToast(data?.message || "Erro ao concluir o ticket.", "error");
        }
    } catch (error) {
        console.error(error);
        showToast("Erro ao tentar concluir o ticket.", "error");
    }
}

async function deleteTicket(id) {
    const isConfirmed = confirm("Tem certeza que deseja excluir este ticket? Esta ação não pode ser desfeita.");
    
    if (isConfirmed) {
        try {
            const response = await fetch(`/api/ticketsapi/${id}`, { method: 'DELETE' });
            if (response.ok) {
                showToast("Ticket excluído com sucesso!", "success");
                await loadTickets();
            } else {
                showToast("Erro ao excluir o ticket.", "error");
            }
        } catch (error) {
            console.error(error);
            showToast("Erro ao tentar excluir o ticket.", "error");
        }
    }
}


// ===== Ações - viewDetails(id) (LÓGICA DOS BOTÕES ATUALIZADA) =====
async function viewDetails(id) {
    try {
        const response = await fetch(`/api/ticketsapi/${id}`);
        if (!response.ok) throw new Error('Ticket não encontrado');
        const ticket = await response.json();

        const dataAberturaFormatada = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {
            year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'
        });

        const dataAtribuicaoFormatada = ticket.dataAtribuicao 
            ? new Date(ticket.dataAtribuicao).toLocaleString('pt-BR', {
                  year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'
              })
            : "Não atribuído";

        document.getElementById("modalTitle").textContent = `#${ticket.id} - ${ticket.title}`;
        document.getElementById("modalId").textContent = ticket.id;
        document.getElementById("modalUser").textContent = ticket.assignedTo || "Não atribuído";
        document.getElementById("modalSolicitante").textContent = ticket.solicitante || "Não informado";
        document.getElementById("modalCategory").textContent = ticket.category;
        document.getElementById("modalPriority").textContent = ticket.priority;
        document.getElementById("modalStatus").textContent = ticket.status;
        document.getElementById("modalDate").textContent = dataAberturaFormatada;
        document.getElementById("modalAssignedDate").textContent = dataAtribuicaoFormatada;
        document.getElementById("modalDescription").textContent = ticket.description;

        const finalizationInfo = document.getElementById("finalizationInfo");
        const modalFinalizedDate = document.getElementById("modalFinalizedDate");

        if (ticket.dataFechamento) {
            const dataFechamentoFormatada = new Date(ticket.dataFechamento).toLocaleString('pt-BR', {
                year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'
            });
            modalFinalizedDate.textContent = dataFechamentoFormatada;
            finalizationInfo.style.display = 'block';
        } else {
            modalFinalizedDate.textContent = '';
            finalizationInfo.style.display = 'block';
        }

        // ===== Lógica para mostrar/esconder botões e adicionar event listeners (ATUALIZADA AQUI) =====
        
        let approveBtn = document.getElementById("approveBtn");
        let concludeBtn = document.getElementById("concludeBtn");

        // Reseta a visibilidade dos botões para garantir que comecem escondidos
        approveBtn.style.display = 'none';
        concludeBtn.style.display = 'none';

        // Clonagem e Reatribuição (Corrige o problema de event listener duplicado)
        approveBtn.replaceWith(approveBtn.cloneNode(true));
        concludeBtn.replaceWith(concludeBtn.cloneNode(true));

        // Reatribui as referências aos botões limpos
        approveBtn = document.getElementById("approveBtn");
        concludeBtn = document.getElementById("concludeBtn");

        // LÓGICA DE EXIBIÇÃO ATUALIZADA
        if (ticket.status === 'Aberto') {
            // Se está aberto e NÃO tem analista atribuído, mostre Atender
            if (!ticket.assignedToId) {
                approveBtn.style.display = 'block';
                approveBtn.addEventListener('click', () => approveTicket(id));
            }
        } else if (ticket.status === 'Em Andamento') {
            // Se está em andamento E está atribuído ao usuário logado
            if (ticket.assignedToId && ticket.assignedToId.toString() === CURRENT_USER_ID.toString()) {
                concludeBtn.style.display = 'block';
                concludeBtn.addEventListener('click', () => concludeTicket(id));
            }
        }
        
        // ===== Fim da lógica dos botões =====

        document.getElementById("ticketModal").style.display = "flex";
    } catch (error) {
        console.error("Erro ao carregar detalhes do ticket:", error);
        alert("Não foi possível carregar os detalhes do ticket.");
    }
}


// Fechar modal
document.querySelector(".close-btn").addEventListener("click", () => {
    document.getElementById("ticketModal").style.display = "none";
});

window.addEventListener("click", (event) => {
    const modal = document.getElementById("ticketModal");
    if (event.target === modal) {
        modal.style.display = "none";
    }
});

// Garante que o script só roda quando a página estiver totalmente carregada
document.addEventListener("DOMContentLoaded", () => {
    // Carrega os tickets ao iniciar a página
    loadTickets();

    // Adiciona o listener para o botão "Meus Tickets" de forma segura
    const myTicketsButton = document.getElementById("myTicketsButton");
    if (myTicketsButton) {
        myTicketsButton.addEventListener("click", () => {
            isMyTicketsView = !isMyTicketsView;
            loadTickets();
        });
    }

    // Adiciona listeners para os outros filtros
    const searchInput = document.getElementById("searchInput");
    if (searchInput) searchInput.addEventListener("input", loadTickets);

    const statusFilter = document.getElementById("statusFilter");
    if (statusFilter) statusFilter.addEventListener("change", loadTickets);

    const priorityFilter = document.getElementById("priorityFilter");
    if (priorityFilter) priorityFilter.addEventListener("change", loadTickets);

    const dateFilter = document.getElementById("dateFilter");
    if (dateFilter) dateFilter.addEventListener("change", loadTickets);
});