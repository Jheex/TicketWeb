// Função para alternar entre os temas claro e escuro
function toggleTheme() {
    // Obtém o tema atual do atributo "data-theme" do HTML
    const currentTheme = document.documentElement.getAttribute("data-theme");
    
    // Define o novo tema (se estiver escuro, muda para claro, e vice-versa)
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", newTheme);

    // Salva a preferência do usuário no localStorage
    localStorage.setItem("theme", newTheme);

    // Atualiza o ícone do botão de tema
    const icon = document.getElementById("theme-icon");
    if (icon) {
        icon.className = newTheme === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
    }
}

// Executa ao carregar a página para aplicar o tema salvo anteriormente
(function () {
    const savedTheme = localStorage.getItem("theme") || "dark"; // padrão escuro
    document.documentElement.setAttribute("data-theme", savedTheme);

    // Atualiza o ícone do botão de tema conforme o tema salvo
    const icon = document.getElementById("theme-icon");
    if (icon) {
        icon.className = savedTheme === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
    }
})();
