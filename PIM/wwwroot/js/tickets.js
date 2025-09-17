// ===== Função principal para carregar tickets do backend =====
async function loadTickets() {
    // Chama a API real
    const response = await fetch('/api/ticketsapi');
    const tickets = await response.json();

    // Captura filtros e pesquisa
    const searchTerm = document.getElementById("searchInput").value.toLowerCase();
    const statusFilter = document.getElementById("statusFilter").value;
    const priorityFilter = document.getElementById("priorityFilter").value;
    const dateFilter = document.getElementById("dateFilter").value;

    // Filtra tickets
    const filteredTickets = tickets.filter(ticket => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchTerm) 
            || (ticket.assignedTo ? ticket.assignedTo.toLowerCase().includes(searchTerm) : false);

        const matchesStatus = statusFilter === "" || ticket.status === statusFilter;
        const matchesPriority = priorityFilter === "" || ticket.priority === priorityFilter;

        // Lógica do Filtro de Data
        const matchesDate = dateFilter === "all" || new Date(ticket.dataAbertura) >= new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);

        return matchesSearch && matchesStatus && matchesPriority && matchesDate;
    });

    // Container onde os cards serão renderizados
    const container = document.getElementById("ticketContainer");
    container.innerHTML = "";

    // Ordena do mais recente para o mais antigo
    filteredTickets.sort((a, b) => b.id - a.id).forEach(ticket => {
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
        if (ticket.status === 'Aberto' || ticket.status === 'Em Andamento') {
            actionButtonsHtml = `
                <button class="ticket-btn ticket-btn-approve" onclick="approveTicket(${ticket.id})">Aprovar</button>
                <button class="ticket-btn ticket-btn-reject" onclick="rejectTicket(${ticket.id})">Rejeitar</button>
            `;
        }
        
        // Formata a data de abertura para "dd/MM/yyyy HH:mm"
        const dataAbertura = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });

        card.innerHTML = `
            <div class="card-content">
                <div class="card-header">
                    <div class="card-title">#${ticket.id} - ${ticket.title}</div>
                    <button class="delete-btn" onclick="deleteTicket(${ticket.id})">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                            <path d="M3 6l2-2h4V2h6v2h4l2 2v2H3V6zM5 8h14v12a2 2 0 01-2 2H7a2 2 0 01-2-2V8zm6 4v6h2v-6h-2zm-4 0v6h2v-6H7zm8 0v6h2v-6h-2z" />
                        </svg>
                    </button>
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

// ===== Funções de ação =====
async function approveTicket(id) {
    await fetch(`/api/ticketsapi/approve/${id}`, { method: 'POST' });
    loadTickets();
}

async function rejectTicket(id) {
    await fetch(`/api/ticketsapi/reject/${id}`, { method: 'POST' });
    loadTickets();
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
        if (!response.ok) {
            throw new Error('Ticket não encontrado');
        }
        const ticket = await response.json();

        // Formata as datas para o modal
        const dataAberturaFormatada = new Date(ticket.dataAbertura).toLocaleString('pt-BR', {
            year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
        });

        // Preenche o modal com os dados
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
                year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
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

document.querySelector(".close-btn").addEventListener("click", () => {
    document.getElementById("ticketModal").style.display = "none";
});

window.addEventListener("click", (event) => {
    const modal = document.getElementById("ticketModal");
    if (event.target === modal) {
        modal.style.display = "none";
    }
});

document.getElementById("searchInput").addEventListener("input", loadTickets);
document.getElementById("statusFilter").addEventListener("change", loadTickets);
document.getElementById("priorityFilter").addEventListener("change", loadTickets);
document.getElementById("dateFilter").addEventListener("change", loadTickets);

document.addEventListener("DOMContentLoaded", loadTickets);