// =======================================================
// LÓGICA DE TEMA (LIGHT/DARK)
// Requer que o botão tenha id="theme-toggle" e o ícone id="theme-icon"
// =======================================================

// Toggle de tema light/dark
function toggleTheme() {
    const html = document.documentElement;
    const currentTheme = html.getAttribute("data-theme") || "light";
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    
    html.setAttribute("data-theme", newTheme);
    localStorage.setItem("theme", newTheme);

    const icon = document.getElementById("theme-icon");
    updateThemeIcon(newTheme, icon);
}

function updateThemeIcon(theme, iconElement) {
    if (!iconElement) return; // Proteção contra elemento não encontrado

    // Assume o uso de Bootstrap Icons (bi-)
    if (theme === "dark") {
        iconElement.classList.remove("bi-sun-fill");
        iconElement.classList.add("bi-moon-fill");
    } else {
        iconElement.classList.remove("bi-moon-fill");
        iconElement.classList.add("bi-sun-fill");
    }
}


// =======================================================
// FUNÇÕES PRINCIPAIS AO CARREGAR A PÁGINA
// =======================================================

document.addEventListener("DOMContentLoaded", () => {
    
    // --- 1. Inicializa o Tema Salvo e Listener de Clique ---
    
    // Aplica o tema salvo do localStorage
    const savedTheme = localStorage.getItem("theme") || "light";
    document.documentElement.setAttribute("data-theme", savedTheme);

    // Encontra e atualiza o ícone
    const icon = document.getElementById("theme-icon");
    updateThemeIcon(savedTheme, icon);

    // Adiciona o listener de clique ao botão de alternância de tema
    const themeToggle = document.getElementById("theme-toggle");
    if (themeToggle) {
        // Assume que o botão de tema no _Header.cshtml tem id="theme-toggle"
        themeToggle.addEventListener("click", toggleTheme);
    }


    // --- 2. 🚀 Lógica do Modal de Confirmação de Logout (CORRIGIDO) ---
    // Ativa o envio do formulário de Log Out quando o usuário confirma.
    
    const btnConfirmLogout = document.getElementById('btnConfirmLogout'); // Botão "Sim, Sair" no modal

    if (btnConfirmLogout) {
        btnConfirmLogout.addEventListener('click', function(e) {
            e.preventDefault(); 
            
            // Encontrar o formulário de Log Out pelo ID (do _Header.cshtml)
            const logoutForm = document.getElementById('logoutForm');
            
            if (logoutForm) {
                // Envia o formulário (POST) para /Account/Logout
                logoutForm.submit();
            } else {
                // Fallback, caso o formulário não seja encontrado (menos seguro)
                console.error("Formulário de Log Out não encontrado! Redirecionando como fallback.");
                window.location.href = '/Account/Logout'; 
            }
        });
    }

    // --- 3. Outras funções do site (MÁSCARAS, etc.) ---
    // Se você tiver outras funções globais, adicione-as aqui.
});