// Arquivo: wwwroot/js/relatorio.js - VERSÃO FINAL COM TODAS AS CORREÇÕES

// URL do seu endpoint no Controller C#
const API_URL = '/RelatorioGerencial/ObterDados';

// =======================================================
// CORES E CONFIGURAÇÕES DE GRÁFICOS
// =======================================================

const CHART_COLORS = {
    // Paleta Principal
    primary: '#8C6AFF',
    secondary: '#5C3CFF',
    accent: '#FF7F50',

    // Cores de Status e Fatias do Gráfico
    danger: '#FF6B6B', // Vermelho: Em Aberto
    warning: '#FFD93D', // Amarelo: Em Andamento
    success: '#6BCB77', // Verde: Concluídos
    info: '#4D96FF', // Azul: Info/Outros

    text: '#E0E0E0',
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
            font: { size: 18, weight: 'bold' }
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
// FUNÇÕES DE UTILIDADE, PRÉ-PROCESSAMENTO E RENDERIZAÇÃO
// =======================================================

function animateCount(element, duration = 1000) {
    const end = parseFloat(element.getAttribute('data-valor'));
    let startTime = null;
    const start = 0;

    if (isNaN(end)) return;

    const endString = String(end);
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


function updateSection(sectionId, data) {
    const section = document.getElementById(sectionId);
    if (!section) return;

    const map = {
        'resumo': {
            'abertos': '.card-abertos .count',
            'andamento': '.card-andamento .count',
            'finalizados': '.card-finalizados .count', 
            'taxaConclusao': '.card-conclusao .count'
        },
        'kpis': {
            'sla': '.card-sla .count',
            'mttr': '.card-mttr .count',
            'eficiencia': '.card-eficiencia .count'
        }
    };

    const targetMap = map[sectionId] || {};

    for (const apiField in targetMap) {
        const selector = targetMap[apiField];
        const element = section.querySelector(selector);
        
        let value = data[apiField];

        if (element) {
            const numericValue = data[apiField] !== undefined && data[apiField] !== null ? parseFloat(data[apiField]) : 0;
            
            element.setAttribute('data-valor', numericValue);
            
             // 🛑 CORREÇÃO KPI: Se o valor for 0 (numericValue), ele é permitido.
             // Apenas exibe N/A se o valor for indefinido ou nulo (value).
             if (sectionId === 'kpis' && (value === undefined || value === null)) {
                 element.textContent = 'N/A';
                 continue; 
             }

            if (section.classList.contains('active')) {
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
    // Note: Assumimos que a API retorna um objeto 'data' com as propriedades 'resumo', 'kpis', 'tecnicos', 'categorias'
    // Se a sua API retorna apenas um grande objeto, pode ser necessário ajustar a forma como 'data' é mapeado aqui.
    updateSection('resumo', data);
    updateSection('kpis', data); 

    const tecnicosSection = document.getElementById('tecnicos');
    const categoriasSection = document.getElementById('categorias');

    if (tecnicosSection && tecnicosSection.classList.contains('active')) {
        renderGraficoTecnicos(data.tecnicos); 
    } else if (categoriasSection && categoriasSection.classList.contains('active')) {
        // Aplica o pré-processamento de categorias para unificar Network e Rede/Segurança
        const categoriasUnificadas = preprocessCategorias(data.categorias);
        renderGraficoCategorias(categoriasUnificadas); 
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
                    backgroundColor: CHART_COLORS.success, 
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
                    },
                    ...barAesthetics 
                },
                
                // DATASET 2: Em Andamento (Topo da pilha)
                { 
                    label: 'Em Andamento', 
                    data: andamento, 
                    backgroundColor: CHART_COLORS.warning, 
                    stack: 'Stack 0', 
                    order: 1, 
                    // Rótulo interno da barra de Em Andamento
                    datalabels: { 
                        display: (context) => context.dataset.data[context.dataIndex] > 0,
                        color: CHART_COLORS.background, 
                        font: { weight: 'bold', size: 12 },
                        align: 'center',
                        anchor: 'center',
                        formatter: (value) => value > 0 ? value : '',
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
                    text: 'Produtividade por Técnico (Andamento + Concluídos)' 
                },

                // Configuração de DataLabels para o Rótulo de TOTAL no topo
                datalabels: {
                    display: function(context) {
                        const isLastInStack = context.datasetIndex === 1; // Index 1 é 'Em Andamento'
                        const total = totaisProdutividade[context.dataIndex];

                        if (total === 0 || !isLastInStack) return false;
                        
                        return true; 
                    },
                    
                    formatter: (value, context) => {
                         const total = totaisProdutividade[context.dataIndex];
                         return total > 0 ? total : '';
                    },
                    
                    color: CHART_COLORS.text,
                    backgroundColor: CHART_COLORS.background,
                    borderRadius: 4,
                    padding: 4,
                    anchor: 'end',
                    align: 'end',
                    offset: 8, 
                    font: { weight: 'bold', size: 14 },
                }
            }
        }
    });
}

function renderGraficoCategorias(categoriasData) {
    const ctxCat = document.getElementById('graficoCategorias');
    if (!ctxCat) return;

    if (graficoCategoriasInstance) graficoCategoriasInstance.destroy();

    const labels = categoriasData.map(c => c.categoria);
    const values = categoriasData.map(c => c.total);
    const total = values.reduce((a, b) => a + b, 0);
    const backgroundColors = [CHART_COLORS.danger, CHART_COLORS.warning, CHART_COLORS.success, CHART_COLORS.info, CHART_COLORS.primary, CHART_COLORS.secondary, CHART_COLORS.accent, '#A06CD5'];


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
                    position: 'right',
                },
                title: {
                    ...darkChartConfig.plugins.title,
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
            switchTab(btn, target);
        });
    });

    document.querySelector('.export-pdf')?.addEventListener('click', () => alert('Funcionalidade de Exportação de PDF em desenvolvimento!'));
    document.querySelector('.export-excel')?.addEventListener('click', () => alert('Funcionalidade de Exportação de CSV/Excel em desenvolvimento!'));
    document.querySelector('.export-print')?.addEventListener('click', () => window.print());
}

function switchTab(btn, targetId) {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(b => b.classList.remove('active'));
    tabContents.forEach(c => c.classList.remove('active'));

    btn.classList.add('active');
    const activeSection = document.getElementById(targetId);
    activeSection?.classList.add('active');

    // Recarrega os dados e redesenha o gráfico da aba ATUAL
    applyFilters(); 
}

document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();

    const initialButton = document.querySelector('.tab-btn.active');
    if (initialButton) {
        const target = initialButton.getAttribute('data-target');
        setTimeout(() => {
            switchTab(initialButton, target);
        }, 100);
    } else {
        applyFilters();
    }
});