@section Scripts {
    <script>
        // Função para alternar o tema
        function toggleTheme() {
            const currentTheme = document.documentElement.getAttribute("data-theme");
            const newTheme = currentTheme === "dark" ? "light" : "dark";
            document.documentElement.setAttribute("data-theme", newTheme);
            localStorage.setItem("theme", newTheme);
        }

        // Ao carregar a página, usa o tema salvo
        (function () {
            const savedTheme = localStorage.getItem("theme") || "dark";
            document.documentElement.setAttribute("data-theme", savedTheme);
        })();
    </script>
    <partial name="_ValidationScriptsPartial" />
}
