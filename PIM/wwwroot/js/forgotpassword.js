const form = document.getElementById('forgotPasswordForm');
form.addEventListener('submit', function(e) {
    e.preventDefault(); // impede o envio real
    const emailInput = document.getElementById('email');
    let email = emailInput.value.trim();

    if(email.length > 40) {
        alert("O e-mail não pode ter mais que 60 caracteres.");
        emailInput.focus();
        return;
    }

    // Substitui o formulário por mensagem de sucesso
    const wrapper = document.getElementById('form-wrapper');
    wrapper.innerHTML = `
        <div class="form-logo">
            <i class="bi bi-check-circle" style="font-size:48px; color: var(--color-primary);"></i>
        </div>
        <div class="form-header">
            <h2>E-mail Enviado!</h2>
            <p class="subtitle">
                Foi enviado um e-mail de verificação para: <strong>${email}</strong>.<br/>
                Siga as instruções no e-mail para redefinir sua senha.
            </p>
            <button onclick="window.location.href='/Account/Login'" class="btn btn-primary">Voltar ao Login</button>
        </div>
    `;
});
