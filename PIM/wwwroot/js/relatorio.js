// Arquivo: wwwroot/js/relatorio.js - VERSÃO FINAL COMPLETA

// URL do seu endpoint no Controller C#
// OBS: Você precisará implementar o endpoint /RelatorioGerencial/ObterDados no seu Controller
const API_URL = '/RelatorioGerencial/ObterDados';

// =======================================================
// CORES E CONFIGURAÇÕES DE GRÁFICOS
// =======================================================

const CHART_COLORS = {
    // Paleta Principal (Tema Roxo)
    primary: '#8C6AFF', // Roxo Principal (Mais Vibrante)
    secondary: '#5C3CFF', // Roxo Secundário (Original, usado como fallback)

    // Cores de Status e Fatias do Gráfico
    danger: '#FF6B6B',
    warning: '#FFD93D',
    success: '#6BCB77',
    info: '#4D96FF',
    accent: '#FF7F50',

    text: '#E0E0E0', // Branco/Cinza claro para texto (garante contraste)
    grid: 'rgba(224, 224, 255, 0.15)',
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
// FUNÇÕES DE EXPORTAÇÃO E IMPRESSÃO
// =======================================================

/**
 * Exporta o gráfico como um arquivo PDF, garantindo fundo escuro.
 * Requer as bibliotecas jspdf e html2canvas.
 */
function exportChartToPDF(canvasId, filename) {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !window.jspdf || !window.jspdf.jsPDF) {
        console.error(`Canvas com ID ${canvasId} ou biblioteca jspdf não encontrado.`);
        alert("Erro: Bibliotecas de exportação (jsPDF) não carregadas. Verifique a seção 'Scripts' no seu HTML.");
        return;
    }

    const imgData = canvas.toDataURL('image/png', 1.0);
    const pdf = new window.jspdf.jsPDF({
        orientation: 'landscape', 
    });

    const pdfWidth = pdf.internal.pageSize.getWidth();
    const pdfHeight = pdf.internal.pageSize.getHeight();
    
    // Fundo Escuro (Cor do tema principal: #1A0D3A)
    const backgroundColor = '#1A0D3A';
    pdf.setFillColor(backgroundColor);
    pdf.rect(0, 0, pdfWidth, pdfHeight, 'F');

    const imgProps = pdf.getImageProperties(imgData);
    const imgWidth = pdfWidth - 20;
    const imgHeight = (imgProps.height * imgWidth) / imgProps.width;
    
    const x = 10;
    const y = (pdfHeight - imgHeight) / 2;

    pdf.addImage(imgData, 'PNG', x, y, imgWidth, imgHeight);
    pdf.save(`${filename}.pdf`);
}


/**
 * Exporta os dados do gráfico para um arquivo CSV (Excel).
 */
function exportDataToCSV(chartId, filename) {
    let chartInstance = null;

    if (chartId === 'graficoTecnicos') {
        chartInstance = graficoTecnicosInstance;
    } else if (chartId === 'graficoCategorias') {
        chartInstance = graficoCategoriasInstance;
    }

    if (!chartInstance) {
        console.error(`Instância do gráfico ${chartId} não encontrada.`);
        alert("Erro: Dados do gráfico não estão disponíveis para exportação CSV.");
        return;
    }

    const { labels } = chartInstance.data;
    const { datasets } = chartInstance.data;
    
    if (!labels || !datasets || datasets.length === 0) {
        console.error("Dados do gráfico estão vazios.");
        return;
    }

    // Cria o cabeçalho usando ; como separador (padrão Excel PT-BR)
    const headers = ["Item", ...datasets.map(d => d.label)];
    const csvRows = [headers.join(';')];

    // Preenche as linhas de dados
    labels.forEach((label, index) => {
        const row = [
            `"${label}"`,
            ...datasets.map(d => d.data[index] || 0)
        ];
        csvRows.push(row.join(';'));
    });

    // Converte para Blob e dispara o download (com BOM para acentuação)
    const csvString = csvRows.join('\n');
    const blob = new Blob(["\uFEFF" + csvString], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement("a");
    
    link.href = URL.createObjectURL(blob);
    link.download = `${filename}.csv`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

/**
 * Dispara a impressão da aba ativa.
 * A visibilidade será controlada pelo @media print no CSS.
 */
function printActiveGraph() {
    window.print();
}

// =======================================================
// FUNÇÕES DE UTILIDADE E PROCESSAMENTO DE DADOS
// =======================================================

function animateCount(element, duration = 1000) {
    const end = parseFloat(element.getAttribute('data-valor'));
    let startTime = null;
    const start = 0;

    if (isNaN(end)) return;

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


function preprocessCategorias(categoriasData) {
    const unifiedMap = new Map();
    const unificationTargets = ['Network', 'Rede/Segurança'];
    const unifiedName = 'Rede e Segurança';

    categoriasData.forEach(item => {
        let categoryName = item.categoria;

        if (unificationTargets.includes(categoryName)) {
            categoryName = unifiedName;
        }

        const currentTotal = unifiedMap.get(categoryName) || 0;
        unifiedMap.set(categoryName, currentTotal + item.total);
    });

    const unifiedData = [];
    unifiedMap.forEach((total, categoria) => {
        unifiedData.push({ categoria, total });
    });

    return unifiedData;
}


function sortCategorias(data) {
    const dataCopy = [...data];
    const outrosIndex = dataCopy.findIndex(item => item.categoria.toLowerCase() === 'outros');
    let outrosItem = null;

    if (outrosIndex !== -1) {
        outrosItem = dataCopy.splice(outrosIndex, 1)[0];
    }

    dataCopy.sort((a, b) => a.categoria.localeCompare(b.categoria));

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

    const MAP_FIELDS = {
        'resumo': {
            abertos: '.card-abertos .count',
            andamento: '.card-andamento .count',
            finalizados: '.card-finalizados .count',
            taxaConclusao: '.card-conclusao .count'
        }
    };

    const targetMap = MAP_FIELDS[sectionId] || {};
    const isActive = section.classList.contains('active');

    for (const apiField in targetMap) {
        const selector = targetMap[apiField];
        const element = section.querySelector(selector);
        const value = data[apiField];

        if (element) {
            const numericValue = value !== undefined && value !== null ? parseFloat(value) : 0;

            element.setAttribute('data-valor', numericValue);

            if (isActive) {
                if (typeof numericValue !== 'number' || isNaN(numericValue)) {
                    element.textContent = value;
                } else {
                    animateCount(element);
                }
            } else {
                let decimalPlaces = (apiField === 'taxaConclusao') ? 1 : 0;
                let displayValue = typeof numericValue === 'number' && !isNaN(numericValue) ? numericValue.toFixed(decimalPlaces) : value;

                if (apiField === 'taxaConclusao') displayValue += '%';

                element.textContent = displayValue;
            }
        }
    }
}


function updateDashboard(data) {
    updateSection('resumo', data);

    const tecnicosSection = document.getElementById('tecnicos');
    const categoriasSection = document.getElementById('categorias');

    if (tecnicosSection && tecnicosSection.classList.contains('active')) {
        renderGraficoTecnicos(data.tecnicos);
    } 

    if (categoriasSection && categoriasSection.classList.contains('active')) {
          const categoriasUnificadas = preprocessCategorias(data.categorias);
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
                {
                    label: 'Concluídos',
                    data: finalizados,
                    backgroundColor: '#4A2B99',
                    stack: 'Stack 0',
                    order: 2,
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
                {
                    label: 'Em Andamento',
                    data: andamento,
                    backgroundColor: CHART_COLORS.primary,
                    stack: 'Stack 0',
                    order: 1,
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
                    text: 'Produtividade por Técnico'
                },
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

    const labels = categoriasData.map(c => c.categoria);
    const values = categoriasData.map(c => c.total);
    const total = values.reduce((a, b) => a + b, 0);

    const backgroundColors = [
        CHART_COLORS.danger,
        CHART_COLORS.warning,
        CHART_COLORS.success,
        CHART_COLORS.info,
        CHART_COLORS.primary,
        CHART_COLORS.secondary,
        CHART_COLORS.accent,
        '#A06CD5'
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
                    position: 'bottom',
                    labels: {
                        ...darkChartConfig.plugins.legend.labels,
                        padding: 25,
                        boxWidth: 20
                    }
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
    // Simulando chamada à API. Implemente a lógica real aqui.
    try {
        const url = `${API_URL}?periodo=${periodo}&tecnico=${tecnico}`;
        const response = await fetch(url);
        if (!response.ok) throw new Error(`Erro ao buscar dados do servidor: ${response.status}`);

        const data = await response.json();
        updateDashboard(data);

    } catch (error) {
        console.error('Erro ao carregar dados do relatório:', error);
        // Pode-se adicionar uma função para mostrar dados mockados em caso de erro da API
    }
}

function applyFilters() {
    // Por enquanto, filtros fixos, mas aqui é onde você leria o estado da UI (seletors)
    const dataFiltro = '30d';
    const tecnicoFiltro = 'todos';

    carregarDadosRelatorio(dataFiltro, tecnicoFiltro);
}


function setupEventListeners() {
    const tabButtons = document.querySelectorAll('.tab-btn');

    tabButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.getAttribute('data-target');
            switchTab(btn, target, true); 
        });
    });

    // Event listeners para Exportar PDF
    document.querySelectorAll('.export-pdf').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const target = e.currentTarget.getAttribute('data-export-target');
            const [canvasId] = target.split('-');
            const filename = canvasId === 'graficoTecnicos' ? 'Relatorio_Tecnicos' : 'Relatorio_Categorias';
            
            exportChartToPDF(canvasId, filename);
        });
    });

    // Event listeners para Exportar CSV/Excel
    document.querySelectorAll('.export-excel').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const target = e.currentTarget.getAttribute('data-export-target');
            const [chartId] = target.split('-');
            const filename = chartId === 'graficoTecnicos' ? 'Dados_Tecnicos' : 'Dados_Categorias';
            
            exportDataToCSV(chartId, filename);
        });
    });

    // Event listeners para Imprimir Gráfico (Ctrl+P)
    document.querySelectorAll('.export-print').forEach(btn => {
        btn.addEventListener('click', () => {
             printActiveGraph();
        });
    });
}


function switchTab(btn, targetId, shouldLoadData = false) {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(b => b.classList.remove('active'));
    tabContents.forEach(c => c.classList.remove('active'));

    btn.classList.add('active');
    const activeSection = document.getElementById(targetId);
    activeSection?.classList.add('active');

    // Força o recarregamento dos dados apenas se a aba for trocada
    if (shouldLoadData) { 
        applyFilters(); 
    }
}

// Inicialização: Configura os listeners e carrega os dados iniciais
document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();

    const initialButton = document.querySelector('.tab-btn.active');
    
    if (initialButton) {
        const target = initialButton.getAttribute('data-target');
        // A primeira carga de dados ocorre aqui ou via applyFilters
        switchTab(initialButton, target, false); 
    }
    
    // Força a primeira carga de dados e renderização do dashboard
    applyFilters(); 
});