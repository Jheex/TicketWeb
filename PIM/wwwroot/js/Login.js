function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute("data-theme");
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", newTheme);
    localStorage.setItem("theme", newTheme);

    const icon = document.getElementById("theme-icon");
    if (icon) {
        icon.className = newTheme === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
    }
}

// Ao carregar a página, usa o tema salvo
(function () {
    const savedTheme = localStorage.getItem("theme") || "dark";
    document.documentElement.setAttribute("data-theme", savedTheme);
    const icon = document.getElementById("theme-icon");
    if (icon) {
        icon.className = savedTheme === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
    }
})();
