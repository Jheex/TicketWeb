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
            || (ticket.assignedTo ? ticket.assignedTo.toLowerCase().includes(searchTerm) : false);

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

        let actionButtonsHtml = '';
        if (ticket.status === 'Aberto') {
            actionButtonsHtml = `
                <button class="ticket-btn ticket-btn-approve" onclick="approveTicket(${ticket.id})">Aprovar</button>
                <button class="ticket-btn ticket-btn-reject" onclick="rejectTicket(${ticket.id})">Rejeitar</button>
            `;
        } else if (ticket.status === 'Em Andamento') {
             actionButtonsHtml = `
                <button class="ticket-btn ticket-btn-approve" onclick="approveTicket(${ticket.id})">Concluir</button>
                <button class="ticket-btn ticket-btn-reject" onclick="rejectTicket(${ticket.id})">Rejeitar</button>
            `;
        }

        const dataAbertura = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'});

        card.innerHTML = `
            <div class="card-content">
                <div class="card-header">
                    <div class="card-title">#${ticket.id} - ${ticket.title}</div>
                    <button class="delete-btn" onclick="deleteTicket(${ticket.id})">🗑️</button>
                </div>
                <div><strong>Usuário:</strong> ${ticket.assignedTo || "Não atribuído"}</div>
                <div class="ticket-category"><strong>Categoria:</strong> ${ticket.category}</div>
                <div class="ticket-date"><strong>Solicitado em:</strong> ${dataAbertura}</div>
                <div>
                    <span class="ticket-badge ${priorityClass}">Prioridade: ${ticket.priority}</span>
                    <span class="ticket-badge ${statusClass}">Status: ${ticket.status}</span>
                </div>
            </div>
            <div class="ticket-actions">
                ${actionButtonsHtml}
                <button class="ticket-btn ticket-btn-details" onclick="viewDetails(${ticket.id})">Detalhes</button>
            </div>
        `;
        container.appendChild(card);
    });
}

// ===== Ações =====
async function approveTicket(id) {
    try {
        const response = await fetch(`/api/ticketsapi/approve/${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            alert("Ticket aprovado com sucesso!");
            loadTickets();
        } else {
            alert("Erro ao aprovar o ticket.");
        }
    } catch (error) {
        console.error("Erro na requisição de aprovação:", error);
        alert("Erro ao tentar aprovar o ticket.");
    }
}

async function rejectTicket(id) {
    try {
        const response = await fetch(`/api/ticketsapi/reject/${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            alert("Ticket rejeitado com sucesso!");
            loadTickets();
        } else {
            alert("Erro ao rejeitar o ticket.");
        }
    } catch (error) {
        console.error("Erro na requisição de rejeição:", error);
        alert("Erro ao tentar rejeitar o ticket.");
    }
}

async function deleteTicket(id) {
    const isConfirmed = confirm("Tem certeza que deseja excluir este ticket? Esta ação não pode ser desfeita.");
    
    if (isConfirmed) {
        try {
            const response = await fetch(`/api/ticketsapi/${id}`, { method: 'DELETE' });
            if (response.ok) {
                alert("Ticket excluído com sucesso!");
                loadTickets();
            } else {
                alert("Erro ao excluir o ticket.");
            }
        } catch (error) {
            console.error("Erro na requisição de exclusão:", error);
            alert("Erro ao tentar excluir o ticket.");
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

        document.getElementById("modalTitle").textContent = `#${ticket.id} - ${ticket.title}`;
        document.getElementById("modalId").textContent = ticket.id;
        document.getElementById("modalUser").textContent = ticket.assignedTo;
        document.getElementById("modalCategory").textContent = ticket.category;
        document.getElementById("modalPriority").textContent = ticket.priority;
        document.getElementById("modalStatus").textContent = ticket.status;
        document.getElementById("modalDate").textContent = dataAberturaFormatada;
        document.getElementById("modalDescription").textContent = ticket.description;

        const finalizationInfo = document.getElementById("finalizationInfo");
        if (ticket.dataFechamento) {
            const dataFechamentoFormatada = new Date(ticket.dataFechamento).toLocaleString('pt-BR', {
                year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit'
            });
            document.getElementById("modalFinalizedDate").textContent = dataFechamentoFormatada;
            finalizationInfo.style.display = 'block';
        } else {
            finalizationInfo.style.display = 'none';
        }

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