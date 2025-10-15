// wwwroot/js/footer.js
document.addEventListener('DOMContentLoaded', function () {
  const faqAccordion = document.getElementById('faqAccordion');
  if (!faqAccordion) return;

  const items = Array.from(faqAccordion.querySelectorAll('.accordion-item'));
  const searchInput = document.getElementById('faqSearch');
  const tags = Array.from(document.querySelectorAll('.faq-tag'));
  const openAllBtn = document.getElementById('openAllBtn');
  const closeAllBtn = document.getElementById('closeAllBtn');
  const countEl = document.getElementById('faqCount');
  const STORAGE_KEY = 'faqOpen';

  // normaliza texto (remove acentos e coloca em minúsculas)
  function normalize(str) {
    return (str || '').toString().normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
  }

  // atualiza contador de perguntas visíveis
  function updateCount(n) {
    countEl.textContent = n + (n === 1 ? ' pergunta encontrada' : ' perguntas encontradas');
  }

  // filtra as perguntas do FAQ com base na pesquisa e nas tags
  function filterFaq() {
    const q = normalize(searchInput.value.trim());
    const activeTag = document.querySelector('.faq-tag.active');
    const categoryFilter = activeTag ? activeTag.dataset.filter : 'all';
    let visible = 0;

    items.forEach(item => {
      const btn = item.querySelector('.accordion-button');
      const body = item.querySelector('.accordion-body');
      const qText = normalize(btn.textContent);
      const aText = normalize(body.textContent);
      const categories = (item.dataset.category || '').split(',').map(s => s.trim().toLowerCase());
      const matchCategory = (categoryFilter === 'all') || categories.includes(categoryFilter);
      const matchQuery = q === '' || qText.includes(q) || aText.includes(q);

      if (matchCategory && matchQuery) {
        item.classList.remove('d-none');
        visible++;
      } else {
        // esconde o item e fecha se estiver aberto
        item.classList.add('d-none');
        const collapseEl = item.querySelector('.accordion-collapse');
        if (collapseEl && collapseEl.classList.contains('show')) {
          bootstrap.Collapse.getOrCreateInstance(collapseEl).hide();
        }
      }
    });

    updateCount(visible);
  }

  // clique nas tags
  tags.forEach(tag => {
    tag.addEventListener('click', function () {
      tags.forEach(t => t.classList.remove('active'));
      this.classList.add('active');
      filterFaq();
      searchInput.focus();
    });
  });

  // input de pesquisa
  searchInput.addEventListener('input', filterFaq);

  // abrir / fechar todos (aplica apenas aos visíveis)
  openAllBtn.addEventListener('click', function () {
    items.forEach(item => {
      if (!item.classList.contains('d-none')) {
        const collapseEl = item.querySelector('.accordion-collapse');
        bootstrap.Collapse.getOrCreateInstance(collapseEl).show();
      }
    });
  });

  closeAllBtn.addEventListener('click', function () {
    items.forEach(item => {
      const collapseEl = item.querySelector('.accordion-collapse');
      bootstrap.Collapse.getOrCreateInstance(collapseEl).hide();
    });
  });

  // persiste itens abertos no localStorage
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]');
    if (Array.isArray(saved)) {
      saved.forEach(id => {
        const el = document.getElementById(id);
        if (el) bootstrap.Collapse.getOrCreateInstance(el).show();
      });
    }
  } catch (e) { /* ignora erros */ }

  // salva estado de aberto/fechado
  faqAccordion.addEventListener('shown.bs.collapse', function (e) {
    try {
      const id = e.target.id;
      const arr = JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]');
      if (!arr.includes(id)) {
        arr.push(id);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(arr));
      }
    } catch (err) { /* ignora erros */ }
  });

  faqAccordion.addEventListener('hidden.bs.collapse', function (e) {
    try {
      const id = e.target.id;
      let arr = JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]');
      arr = arr.filter(x => x !== id);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(arr));
    } catch (err) { /* ignora erros */ }
  });

  // navegação pelo teclado (setas)
  const buttons = Array.from(faqAccordion.querySelectorAll('.accordion-button'));
  buttons.forEach((btn, i) => {
    btn.addEventListener('keydown', function (ev) {
      if (ev.key === 'ArrowDown') {
        ev.preventDefault();
        const next = buttons[(i + 1) % buttons.length];
        if (next) next.focus();
      } else if (ev.key === 'ArrowUp') {
        ev.preventDefault();
        const prev = buttons[(i - 1 + buttons.length) % buttons.length];
        if (prev) prev.focus();
      } else if (ev.key === 'Home') {
        ev.preventDefault();
        buttons[0].focus();
      } else if (ev.key === 'End') {
        ev.preventDefault();
        buttons[buttons.length - 1].focus();
      }
    });
  });

  // inicializa filtro ao carregar a página
  filterFaq();
});
