// Arquivo: wwwroot/js/relatorio.js - VERSÃO FINAL (ROXO TEMA DARK MODE COM ORDENAÇÃO)

// URL do seu endpoint no Controller C#
const API_URL = '/RelatorioGerencial/ObterDados';

// =======================================================
// CORES E CONFIGURAÇÕES DE GRÁFICOS
// =======================================================

const CHART_COLORS = {
    // Paleta Principal (Tema Roxo)
    primary: '#8C6AFF', // Roxo Principal (Mais Vibrante)
    secondary: '#5C3CFF', // Roxo Secundário (Original, usado como fallback)

    // Cores de Status e Fatias do Gráfico (mantidas para o gráfico de rosca/categorias)
    danger: '#FF6B6B',
    warning: '#FFD93D',
    success: '#6BCB77',
    info: '#4D96FF',
    accent: '#FF7F50',

    text: '#E0E0E0', // Branco/Cinza claro para texto (garante contraste)
    grid: 'rgba(224, 224, 224, 0.15)',
    background: '#2b3e50' // Fundo do container do gráfico
};

let graficoTecnicosInstance = null;
let graficoCategoriasInstance = null;

// Configurações de Gráficos (Tema Dark Mode Base)
const darkChartConfig = {
    responsive: true,
    maintainAspectRatio: false,
    animation: { duration: 1200, easing: 'easeOutQuart' },
    plugins: {
        legend: {
            labels: { color: CHART_COLORS.text, padding: 20, usePointStyle: true },
            padding: { top: 30 }
        },
        title: {
            display: true,
            color: 'white',
            padding: { top: 0, bottom: 20 },
            // 🛑 NOVO: Aumentando o tamanho da fonte do título para 24
            font: { size: 24, weight: 'bold' }
        },
        tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.7)',
            titleColor: 'white',
            bodyColor: 'white',
            borderColor: CHART_COLORS.primary,
            borderWidth: 1,
        }
    },
    scales: {
        y: {
            // Garante que o eixo Y comece em 0
            beginAtZero: true,
            grid: { color: CHART_COLORS.grid, drawBorder: false, borderDash: [2, 2] },
            ticks: { color: CHART_COLORS.text },
            title: { display: false, text: 'Nº de Chamados', color: CHART_COLORS.text }
        },
        x: {
            grid: { display: false },
            ticks: { color: CHART_COLORS.text },
        }
    }
};

// =======================================================
// FUNÇÕES DE UTILIDADE
// =======================================================

function animateCount(element, duration = 1000) {
    const end = parseFloat(element.getAttribute('data-valor'));
    let startTime = null;
    const start = 0;

    if (isNaN(end)) return;

    // Usa toFixed(1) para percentuais e 0 para inteiros (horas)
    const decimalPlaces = element.textContent.includes('%') ? 1 : 0;

    const textContent = element.textContent || "";
    const suffix = textContent.includes('%') ? '%' :
        textContent.includes('h') ? 'h' : '';

    function update(currentTime) {
        if (!startTime) startTime = currentTime;
        const progress = Math.min((currentTime - startTime) / duration, 1);

        let value = progress * (end - start) + start;

        let displayedValue = value.toFixed(decimalPlaces);
        if (decimalPlaces === 0) {
            displayedValue = Math.floor(value);
        }

        element.textContent = displayedValue + suffix;

        if (progress < 1) {
            requestAnimationFrame(update);
        } else {
            element.textContent = end.toFixed(decimalPlaces) + suffix;
        }
    }
    requestAnimationFrame(update);
}

// =======================================================
// PRÉ-PROCESSAMENTO E ORDENAÇÃO
// =======================================================

/**
 * Unifica categorias com nomes semelhantes (ex: Network e Rede/Segurança).
 * @param {Array<Object>} categoriasData - Dados originais da API.
 * @returns {Array<Object>} Dados com categorias unificadas.
 */
function preprocessCategorias(categoriasData) {
    const unifiedMap = new Map();

    // Lista de categorias que devem ser unificadas sob o mesmo nome
    const unificationTargets = ['Network', 'Rede/Segurança'];
    const unifiedName = 'Rede e Segurança'; // O nome final desejado

    categoriasData.forEach(item => {
        let categoryName = item.categoria;

        if (unificationTargets.includes(categoryName)) {
            categoryName = unifiedName;
        }

        const currentTotal = unifiedMap.get(categoryName) || 0;
        unifiedMap.set(categoryName, currentTotal + item.total);
    });

    // Converte o Map de volta para o formato de array esperado pelo Chart.js
    const unifiedData = [];
    unifiedMap.forEach((total, categoria) => {
        unifiedData.push({ categoria, total });
    });

    return unifiedData;
}


/**
 * Ordena os dados de categoria alfabeticamente, movendo 'Outros' para o final.
 * 🛑 MELHORIA 1: Cria uma cópia para garantir a imutabilidade do array original.
 * @param {Array<Object>} data - Dados de categoria pré-processados.
 * @returns {Array<Object>} Dados de categoria ordenados.
 */
function sortCategorias(data) {
    // Cria uma cópia do array para ordenar sem modificar o original (Imutabilidade)
    const dataCopy = [...data];

    // 1. Encontra e remove "Outros" (case insensitive) da cópia
    const outrosIndex = dataCopy.findIndex(item => item.categoria.toLowerCase() === 'outros');
    let outrosItem = null;

    if (outrosIndex !== -1) {
        // .splice retorna um array, pegamos o primeiro elemento [0]
        outrosItem = dataCopy.splice(outrosIndex, 1)[0];
    }

    // 2. Ordena o restante alfabeticamente
    dataCopy.sort((a, b) => a.categoria.localeCompare(b.categoria));

    // 3. Adiciona "Outros" de volta ao final, se existir
    if (outrosItem) {
        dataCopy.push(outrosItem);
    }

    return dataCopy;
}

// =======================================================
// ATUALIZAÇÃO DO DASHBOARD E KPIS
// =======================================================

function updateSection(sectionId, data) {
    const section = document.getElementById(sectionId);
    if (!section) return;

    // 🛑 MELHORIA 3: Usando const para o mapeamento
    const MAP_FIELDS = {
        'resumo': {
            abertos: '.card-abertos .count',
            andamento: '.card-andamento .count',
            finalizados: '.card-finalizados .count',
            taxaConclusao: '.card-conclusao .count'
        },
        'kpis': {
            sla: '.card-sla .count',
            mttr: '.card-mttr .count',
            eficiencia: '.card-eficiencia .count'
        }
    };

    const targetMap = MAP_FIELDS[sectionId] || {};
    const isActive = section.classList.contains('active');

    for (const apiField in targetMap) {
        // 🛑 MELHORIA 3: Usando desestruturação para obter seletor
        const selector = targetMap[apiField];
        const element = section.querySelector(selector);
        // 🛑 MELHORIA 3: Usando const para valor e destructuring
        const value = data[apiField];

        if (element) {
            const numericValue = value !== undefined && value !== null ? parseFloat(value) : 0;

            element.setAttribute('data-valor', numericValue);

             // Apenas exibe N/A se o valor for indefinido ou nulo
             if (sectionId === 'kpis' && (value === undefined || value === null)) {
                 element.textContent = 'N/A';
                 continue;
             }

            if (isActive) {
                 if (typeof numericValue !== 'number' || isNaN(numericValue)) {
                     element.textContent = value;
                 } else {
                     animateCount(element);
                 }
            } else {
                 // Formatação para exibição rápida quando a aba está inativa
                 let decimalPlaces = (apiField === 'taxaConclusao' || apiField === 'sla') ? 1 : 0;
                 let displayValue = typeof numericValue === 'number' && !isNaN(numericValue) ? numericValue.toFixed(decimalPlaces) : value;

                 if (apiField === 'taxaConclusao' || apiField === 'sla') displayValue += '%';
                 else if (apiField === 'mttr') displayValue += 'h';

                 element.textContent = displayValue;
            }
        }
    }
}


function updateDashboard(data) {
    updateSection('resumo', data);
    updateSection('kpis', data);

    const tecnicosSection = document.getElementById('tecnicos');
    const categoriasSection = document.getElementById('categorias');

    // Sempre tenta desenhar os gráficos se a aba estiver ativa para garantir a visualização correta
    if (tecnicosSection && tecnicosSection.classList.contains('active')) {
        renderGraficoTecnicos(data.tecnicos);
    } 

    if (categoriasSection && categoriasSection.classList.contains('active')) {
         // 1. Aplica o pré-processamento de categorias para unificação
         const categoriasUnificadas = preprocessCategorias(data.categorias);

         // 2. Ordena as categorias (alfabética, com "Outros" por último)
         const categoriasOrdenadas = sortCategorias(categoriasUnificadas);

         renderGraficoCategorias(categoriasOrdenadas);
    }
}

// =======================================================
// RENDERIZAÇÃO DOS GRÁFICOS
// =======================================================

function renderGraficoTecnicos(tecnicosData) {
    const ctxTec = document.getElementById('graficoTecnicos');
    if (!ctxTec) return;

    if (graficoTecnicosInstance) graficoTecnicosInstance.destroy();

    const labels = tecnicosData.map(t => t.tecnico || 'N/A');
    const finalizados = tecnicosData.map(t => t.finalizados || 0);
    const andamento = tecnicosData.map(t => t.andamento || 0);

    // CÁLCULO DE TOTAIS: Apenas Andamento + Finalizados (Produtividade)
    const totaisProdutividade = labels.map((_, index) =>
        finalizados[index] + andamento[index]
    );

    const barAesthetics = {
        barPercentage: 0.9,
        categoryPercentage: 0.8,
    };

    graficoTecnicosInstance = new Chart(ctxTec.getContext('2d'), {
        type: 'bar',
        plugins: [ChartDataLabels],
        data: {
            labels: labels,
            datasets: [
                // DATASET 1: Concluídos (Base da pilha)
                {
                    label: 'Concluídos',
                    data: finalizados,
                    backgroundColor: '#4A2B99', // 🟣 Roxo mais escuro
                    stack: 'Stack 0',
                    order: 2,
                    // Rótulo interno da barra de Concluídos
                    datalabels: {
                        display: (context) => context.dataset.data[context.dataIndex] > 0,
                        color: CHART_COLORS.text,
                        font: { weight: 'bold', size: 12 },
                        align: 'center',
                        anchor: 'center',
                        formatter: (value) => value > 0 ? value : '',
                        textShadowBlur: 2,
                        textShadowColor: 'rgba(0, 0, 0, 0.6)'
                    },
                    ...barAesthetics
                },

                // DATASET 2: Em Andamento (Topo da pilha)
                {
                    label: 'Em Andamento',
                    data: andamento,
                    backgroundColor: CHART_COLORS.primary, // 🟣 Roxo Principal (vibrante)
                    stack: 'Stack 0',
                    order: 1,
                    // Rótulo interno da barra de Em Andamento
                    datalabels: {
                        display: (context) => context.dataset.data[context.dataIndex] > 0,
                        color: CHART_COLORS.text,
                        font: { weight: 'bold', size: 12 },
                        align: 'center',
                        anchor: 'center',
                        formatter: (value) => value > 0 ? value : '',
                        textShadowBlur: 2,
                        textShadowColor: 'rgba(0, 0, 0, 0.6)'
                    },
                    ...barAesthetics
                },
            ]
        },
        options: {
            ...darkChartConfig,

            scales: {
                y: { ...darkChartConfig.scales.y, stacked: true, beginAtZero: true },
                x: { ...darkChartConfig.scales.x, stacked: true }
            },

            plugins: {
                ...darkChartConfig.plugins,
                title: {
                    ...darkChartConfig.plugins.title,
                    // 🛑 NOVO TÍTULO E FONTE MAIOR
                    text: 'Produtividade por Técnico'
                },

                // Configuração de DataLabels para o Rótulo de TOTAL no topo
                datalabels: {
                    display: function(context) {
                        const isLastInStack = context.datasetIndex === 1;
                        const total = totaisProdutividade[context.dataIndex];

                        if (total === 0 || !isLastInStack) return false;

                        return true;
                    },

                    formatter: (value, context) => {
                         const total = totaisProdutividade[context.dataIndex];
                         return total > 0 ? total : '';
                    },

                    // === APARÊNCIA DO TOTAL NO TOPO ===
                    color: '#FFF',
                    backgroundColor: 'rgba(0, 0, 0, 0.6)',
                    borderRadius: 10,
                    padding: { top: 6, bottom: 6, left: 8, right: 8 },
                    anchor: 'end',
                    align: 'end',
                    offset: 8,
                    font: { weight: '900', size: 16 },
                    borderWidth: 1,
                    borderColor: 'rgba(0, 0, 0, 0.4)',
                    textShadowBlur: 2,
                    textShadowColor: 'rgba(0, 0, 0, 0.6)'
                }
            }
        }
    });
}

function renderGraficoCategorias(categoriasData) {
    const ctxCat = document.getElementById('graficoCategorias');
    if (!ctxCat) return;

    if (graficoCategoriasInstance) graficoCategoriasInstance.destroy();

    // Os dados já vêm ordenados (graças à função sortCategorias em updateDashboard)
    const labels = categoriasData.map(c => c.categoria);
    const values = categoriasData.map(c => c.total);
    const total = values.reduce((a, b) => a + b, 0);

    // Cores pré-definidas (serão aplicadas na ordem dos dados)
    const backgroundColors = [
        CHART_COLORS.danger,
        CHART_COLORS.warning,
        CHART_COLORS.success,
        CHART_COLORS.info,
        CHART_COLORS.primary,
        CHART_COLORS.secondary,
        CHART_COLORS.accent,
        '#A06CD5' // Roxo Adicional
    ];


    graficoCategoriasInstance = new Chart(ctxCat.getContext('2d'), {
        type: 'doughnut',
        plugins: [ChartDataLabels],
        data: {
            labels: labels,
            datasets: [{
                label: 'Total de Chamados',
                data: values,
                backgroundColor: backgroundColors.slice(0, labels.length),
                borderColor: CHART_COLORS.background,
                borderWidth: 4,
                hoverOffset: 15
            }]
        },
        options: {
            ...darkChartConfig,
            scales: {},

            plugins: {
                ...darkChartConfig.plugins,
                legend: {
                    ...darkChartConfig.plugins.legend,
                    position: 'bottom', // Mantido a legenda abaixo
                    labels: {
                        ...darkChartConfig.plugins.legend.labels,
                        padding: 25, // Maior espaço entre os itens da legenda
                        boxWidth: 20
                    }
                },
                title: {
                    ...darkChartConfig.plugins.title,
                    // 🛑 FONTE MAIOR
                    text: 'Distribuição de Chamados por Categoria',
                },

                datalabels: {
                    formatter: (value, context) => {
                        if (value === 0) return '';
                        const percentage = ((value / total) * 100).toFixed(1) + '%';
                        return percentage;
                    },
                    color: '#FFF',
                    textShadowBlur: 2,
                    textShadowColor: 'rgba(0, 0, 0, 0.6)',
                    font: { weight: '900', size: 16 },
                    textAlign: 'center'
                }
            }
        }
    });
}


// =======================================================
// LÓGICA DE CARREGAMENTO E INICIALIZAÇÃO
// =======================================================

async function carregarDadosRelatorio(periodo = '30d', tecnico = 'todos') {
    try {
        const url = `${API_URL}?periodo=${periodo}&tecnico=${tecnico}`;
        const response = await fetch(url);
        if (!response.ok) throw new Error(`Erro ao buscar dados do servidor: ${response.status}`);

        const data = await response.json();
        updateDashboard(data);

    } catch (error) {
        console.error('Erro ao carregar dados do relatório:', error);
    }
}

function applyFilters() {
    // Padrão fixo: últimos 30 dias e todos os técnicos.
    const dataFiltro = '30d';
    const tecnicoFiltro = 'todos';

    carregarDadosRelatorio(dataFiltro, tecnicoFiltro);
}


function setupEventListeners() {
    const tabButtons = document.querySelectorAll('.tab-btn');

    tabButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.getAttribute('data-target');
            // O switchTab agora garante que os dados sejam carregados e a aba atualizada
            switchTab(btn, target, true); 
        });
    });

    document.querySelector('.export-pdf')?.addEventListener('click', () => alert('Funcionalidade de Exportação de PDF em desenvolvimento!'));
    document.querySelector('.export-excel')?.addEventListener('click', () => alert('Funcionalidade de Exportação de CSV/Excel em desenvolvimento!'));
    document.querySelector('.export-print')?.addEventListener('click', () => window.print());
}

/**
 * Ativa uma nova aba e, opcionalmente, recarrega os dados.
 * @param {HTMLElement} btn - O botão da aba clicado.
 * @param {string} targetId - O ID da seção de conteúdo a ser ativada.
 * @param {boolean} shouldLoadData - Se deve recarregar os dados (true para clique manual).
 */
function switchTab(btn, targetId, shouldLoadData = false) {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(b => b.classList.remove('active'));
    tabContents.forEach(c => c.classList.remove('active'));

    btn.classList.add('active');
    const activeSection = document.getElementById(targetId);
    activeSection?.classList.add('active');

    // Recarrega os dados APENAS se o switch foi disparado por um clique.
    if (shouldLoadData) { 
        applyFilters(); 
    }
}

// 🛑 MELHORIA 2: Simplificação da Lógica de Inicialização
document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();

    const initialButton = document.querySelector('.tab-btn.active');
    
    // 1. Tenta ativar a aba inicial, mas sem recarregar os dados, apenas a classe 'active'
    if (initialButton) {
        const target = initialButton.getAttribute('data-target');
        // Usamos switchTab com shouldLoadData = false apenas para definir as classes ativas
        switchTab(initialButton, target, false); 
    }
    
    // 2. Sempre carrega os dados uma única vez na inicialização, 
    // e o updateDashboard cuida de renderizar os KPIS e o gráfico da aba ATIVA.
    applyFilters(); 
});