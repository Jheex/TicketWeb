/*----------------DATA DE NASCIMENTO-----------------*/

const dataInput = document.getElementById("DataNascimento");

dataInput.addEventListener("input", function () {
    let val = this.value.replace(/\D/g, "");

    if (val.length > 8) val = val.slice(0, 8);

    let dia = val.slice(0, 2);
    let mes = val.slice(2, 4);
    let ano = val.slice(4, 8);

    if (dia) {
        let d = parseInt(dia, 10);
        if (d > 31) dia = "31";
    }
    if (mes) {
        let m = parseInt(mes, 10);
        if (m > 12) mes = "12";
    }
    if (ano) {
        let a = parseInt(ano, 10);
        if (a > 2025) ano = "2025";
    }

    let resultado = dia;
    if (mes) resultado += "/" + mes;
    if (ano) resultado += "/" + ano;

    this.value = resultado;
});

dataInput.addEventListener("blur", function () {
    let parts = this.value.split("/");
    if (parts.length === 3) {
        let dia = parseInt(parts[0], 10);
        let mes = parseInt(parts[1], 10);
        let ano = parseInt(parts[2], 10);

        if (dia < 1) dia = 1;
        if (dia > 31) dia = 31;
        if (mes < 1) mes = 1;
        if (mes > 12) mes = 12;
        if (ano < 1900) ano = 1900;
        if (ano > 2025) ano = 2025;

        this.value = `${dia.toString().padStart(2,"0")}/${mes.toString().padStart(2,"0")}/${ano}`;
    }
});
/*----------------FINAL DATA DE NACIMENTO-----------------*/


/*----------------TELEFONE-----------------*/

document.addEventListener('DOMContentLoaded', () => {
    const telefoneInput = document.getElementById('Telefone');

    telefoneInput.addEventListener('input', (e) => {
        let value = e.target.value.replace(/\D/g, ''); // Remove tudo que não é número

        // Limita o tamanho máximo
        if (value.length > 11) {
            value = value.slice(0, 11);
        }

        // Aplica máscara (xx) xxxxx-xxxx
        if (value.length > 6) {
            value = `(${value.slice(0, 2)}) ${value.slice(2, 7)}-${value.slice(7)}`;
        } else if (value.length > 2) {
            value = `(${value.slice(0, 2)}) ${value.slice(2)}`;
        } else if (value.length > 0) {
            value = `(${value}`;
        }

        e.target.value = value;
    });
});
/*----------------FINAL TELEFONE-----------------*/
/*----------------EMAIL-----------------*/

document.addEventListener('DOMContentLoaded', () => {
    const emailInput = document.getElementById('Email');
    const emailError = document.getElementById('emailError');

    emailInput.addEventListener('input', (e) => {
        const value = e.target.value;
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // Regex simples para validar email

        if (!regex.test(value) && value.length > 0) {
            emailInput.style.borderColor = '#F04438'; // vermelho
            emailError.style.display = 'block';       // mostra mensagem
        } else {
            emailInput.style.borderColor = '';       // volta para padrão
            emailError.style.display = 'none';       // esconde mensagem
        }
    });
});

/*----------------FINAL EMAIL-----------------*/
/*----------------SENHA-----------------*/

const senha = document.getElementById("Senha");
const confirmar = document.getElementById("ConfirmarSenha");
const confirmarErro = document.getElementById("ConfirmarSenhaError");

function validarSenha() {
    if(confirmar.value !== "" && senha.value !== confirmar.value) {
        confirmarErro.style.display = "block";
    } else {
        confirmarErro.style.display = "none";
    }
}

senha.addEventListener("input", validarSenha);
confirmar.addEventListener("input", validarSenha);
/*----------------FINALIZAR O EDITAR-----------------*/

/*----------------SUBMIT FORM-----------------*/
const form = document.querySelector("form");

form.addEventListener("submit", function(e) {
    let hasError = false;

    // Validação de email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(emailInput.value)) {
        emailError.style.display = "block";
        hasError = true;
    } else {
        emailError.style.display = "none";
    }

    // Validação de senha opcional
    if (!validarSenha()) {
        hasError = true;
    }

    // Bloqueia submit apenas se houver erro
    if (hasError) {
        e.preventDefault();
    }
});
/*----------------FINAL SUBMIT-----------------*/
