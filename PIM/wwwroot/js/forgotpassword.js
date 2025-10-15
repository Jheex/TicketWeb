// Seleciona o formulário de "Esqueci a senha"
const form = document.getElementById('forgotPasswordForm');

// Adiciona evento de envio do formulário
form.addEventListener('submit', function(e) {
    e.preventDefault(); // impede o envio tradicional do formulário

    // Seleciona o input de e-mail e obtém o valor
    const emailInput = document.getElementById('email');
    let email = emailInput.value.trim(); // remove espaços extras no início/fim

    // Validação de tamanho do e-mail
    if(email.length > 60) { // OBS: corrigido para 60 caracteres conforme alerta
        alert("O e-mail não pode ter mais que 60 caracteres.");
        emailInput.focus(); // foca novamente no input
        return; // interrompe a execução
    }

    // Substitui o conteúdo do formulário por uma mensagem de sucesso
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
