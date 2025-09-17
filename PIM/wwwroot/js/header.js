// Toggle de tema light/dark
function toggleTheme() {
    const html = document.documentElement;
    const icon = document.getElementById("theme-icon");

    const currentTheme = html.getAttribute("data-theme") || "light";
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    html.setAttribute("data-theme", newTheme);
    localStorage.setItem("theme", newTheme);

    // Atualiza o ícone
    updateThemeIcon(newTheme, icon);
}

function updateThemeIcon(theme, iconElement) {
    if (!iconElement) return;

    if (theme === "dark") {
        iconElement.classList.remove("bi-sun-fill");
        iconElement.classList.add("bi-moon-fill");
    } else {
        iconElement.classList.remove("bi-moon-fill");
        iconElement.classList.add("bi-sun-fill");
    }
}

// Ao carregar a página
document.addEventListener("DOMContentLoaded", () => {
    const savedTheme = localStorage.getItem("theme") || "light";
    document.documentElement.setAttribute("data-theme", savedTheme);

    const icon = document.getElementById("theme-icon");
    updateThemeIcon(savedTheme, icon);
});