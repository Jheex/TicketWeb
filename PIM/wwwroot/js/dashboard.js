// ==== Inicialização dos filtros com Choices.js ====
const filters = [
    { id: '#status-filter', placeholder: 'Selecione status' },
    { id: '#priority-filter', placeholder: 'Selecione prioridade' },
    { id: '#analyst-filter', placeholder: 'Selecione analista' }
];

const choicesInstances = {};

filters.forEach(filter => {
    const selectElement = document.querySelector(filter.id);
    if (selectElement) {
        choicesInstances[filter.id] = new Choices(selectElement, {
            searchEnabled: true,
            placeholder: true,
            placeholderValue: filter.placeholder,
            removeItemButton: true,
            shouldSort: false
        });
    }
});

// ==== Filtro dependente: Status baseado no Analista selecionado ====
const analystChoices = choicesInstances['#analyst-filter'];
const statusChoices = choicesInstances['#status-filter'];

if (analystChoices && statusChoices) {
    analystChoices.passedElement.element.addEventListener('change', async () => {
        const analystId = analystChoices.getValue(true);

        // Limpa o filtro de status se nenhum analista for selecionado
        if (!analystId || analystId.length === 0) {
            statusChoices.clearStore();
            return;
        }

        try {
            // Faz requisição ao backend para pegar os status disponíveis
            const response = await fetch(`/Dashboard/GetStatusesByAnalyst?analystId=${analystId}`);
            if (!response.ok) {
                throw new Error('Erro ao buscar status do analista.');
            }
            const statuses = await response.json();

            // Formata os dados para o Choices.js
            const choicesData = statuses.map(s => ({ value: s, label: s }));

            // Atualiza as opções do seletor de status
            statusChoices.clearStore();
            statusChoices.setChoices(choicesData, 'value', 'label', true);

        } catch (error) {
            console.error("Falha na busca de status:", error);
        }
    });
}

// ==== Botão "Remover Filtros" ====
const removeFiltersBtn = document.querySelector('.btn-secondary');

if (removeFiltersBtn) {
    removeFiltersBtn.addEventListener('click', (e) => {
        // Redireciona para a URL sem filtros
        window.location.href = removeFiltersBtn.href;
    });
}