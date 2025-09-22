document.addEventListener('DOMContentLoaded', function() {

    /*----------------MÁSCARA DE DATA-----------------*/
    const dataInput = document.getElementById("DataNascimento");
    if (dataInput) {
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
                const currentYear = new Date().getFullYear();
                if (a > currentYear) ano = currentYear.toString();
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
                if (dia < 1 || isNaN(dia)) dia = 1;
                if (dia > 31) dia = 31;
                if (mes < 1 || isNaN(mes)) mes = 1;
                if (mes > 12) mes = 12;
                if (ano < 1900 || isNaN(ano)) ano = 1900;
                const currentYear = new Date().getFullYear();
                if (ano > currentYear) ano = currentYear;
                this.value = `${dia.toString().padStart(2,"0")}/${mes.toString().padStart(2,"0")}/${ano}`;
            }
        });
    }

    /*----------------MÁSCARA DE TELEFONE-----------------*/
    const telefoneInput = document.getElementById('Telefone');
    if (telefoneInput) {
        telefoneInput.addEventListener('input', (e) => {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 11) {
                value = value.slice(0, 11);
            }
            if (value.length > 6) {
                value = `(${value.slice(0, 2)}) ${value.slice(2, 7)}-${value.slice(7)}`;
            } else if (value.length > 2) {
                value = `(${value.slice(0, 2)}) ${value.slice(2)}`;
            } else if (value.length > 0) {
                value = `(${value}`;
            }
            e.target.value = value;
        });
    }
});