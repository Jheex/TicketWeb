// Variável de estado para controlar a visualização
let isMyTicketsView = false;

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
        const matchesDate = dateFilter === "all" || new Date(ticket.dataAbertura) >= new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);

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

// ===== Renderiza tickets na tela =====
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
        "Rejeitado": "badge-status-rejeitado"
    }[ticket.status] || "badge-status-aberto";

    const card = document.createElement("div");
    card.className = "ticket-card";

    const statusBorderClass = {
        "Aberto": "status-border-aberto",
        "Em Andamento": "status-border-emandamento",
        "Concluído": "status-border-concluido",
        "Rejeitado": "status-border-rejeitado"
    }[ticket.status] || "";

    card.classList.add(statusBorderClass);

    // Botões de ação
    let actionButtonsHtml = '';
    if (ticket.status === 'Aberto') {
        actionButtonsHtml = `
            <button class="ticket-btn ticket-btn-approve" onclick="approveTicket(${ticket.id})">Aprovar</button>
            <button class="ticket-btn ticket-btn-reject" onclick="rejectTicket(${ticket.id})">Rejeitar</button>
        `;
    } else if (ticket.status === 'Em Andamento') {
        actionButtonsHtml = `
            <button class="ticket-btn ticket-btn-approve" onclick="concludeTicket(${ticket.id})">Concluir</button>
            <button class="ticket-btn ticket-btn-reject" onclick="rejectTicket(${ticket.id})">Rejeitar</button>
        `;
    }

    const dataAbertura = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'});

    console.log("Ticket:", ticket.id, ticket.dataAtribuicao);
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
            showToast("Ticket aprovado com sucesso!", "success");
            await loadTickets();
            const modal = document.getElementById("ticketModal");
            if (modal.style.display === "flex") viewDetails(id);
        } else {
            showToast("Erro ao aprovar o ticket.", "error");
        }
    } catch (error) {
        console.error(error);
        showToast("Erro ao tentar aprovar o ticket.", "error");
    }
}

async function rejectTicket(id) {
    try {
        const response = await fetch(`/api/ticketsapi/reject/${id}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            showToast("Ticket rejeitado com sucesso!", "success");
            await loadTickets();
            const modal = document.getElementById("ticketModal");
            if (modal.style.display === "flex") viewDetails(id);
        } else {
            showToast("Erro ao rejeitar o ticket.", "error");
        }
    } catch (error) {
        console.error(error);
        showToast("Erro ao tentar rejeitar o ticket.", "error");
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

        // ===== Lógica para mostrar/esconder botões e adicionar event listeners =====
        const approveBtn = document.getElementById("approveBtn");
        const rejectBtn = document.getElementById("rejectBtn");
        const concludeBtn = document.getElementById("concludeBtn");

        // Reseta a visibilidade dos botões para garantir que comecem escondidos
        approveBtn.style.display = 'none';
        rejectBtn.style.display = 'none';
        concludeBtn.style.display = 'none';

        // Remove os event listeners antigos, se existirem
        approveBtn.replaceWith(approveBtn.cloneNode(true));
        rejectBtn.replaceWith(rejectBtn.cloneNode(true));
        concludeBtn.replaceWith(concludeBtn.cloneNode(true));

        // Reatribui as referências aos botões limpos
        const cleanApproveBtn = document.getElementById("approveBtn");
        const cleanRejectBtn = document.getElementById("rejectBtn");
        const cleanConcludeBtn = document.getElementById("concludeBtn");

        // Mostra os botões corretos e adiciona os novos listeners com base no status
        if (ticket.status === 'Aberto') {
            cleanApproveBtn.style.display = 'block';
            cleanRejectBtn.style.display = 'block';
            cleanApproveBtn.addEventListener('click', () => approveTicket(id));
            cleanRejectBtn.addEventListener('click', () => rejectTicket(id));
        } else if (ticket.status === 'Em Andamento') {
            cleanConcludeBtn.style.display = 'block';
            cleanRejectBtn.style.display = 'block';
            cleanConcludeBtn.addEventListener('click', () => concludeTicket(id));
            cleanRejectBtn.addEventListener('click', () => rejectTicket(id));
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

// Função JS para mostrar a notificação
function showToast(message) {
    const toast = document.getElementById("toast");
    toast.textContent = message;
    toast.classList.add("show");

    setTimeout(() => {
        toast.classList.remove("show");
    }, 3000); // dura 3 segundos
}