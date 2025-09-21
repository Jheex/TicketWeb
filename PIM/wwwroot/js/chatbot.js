document.addEventListener("DOMContentLoaded", () => {
    // Referências aos elementos do DOM
    const chatForm = document.getElementById("chat-form");
    const sendBtn = document.getElementById("send-btn");
    const userInput = document.getElementById("user-input");
    const chatMessages = document.getElementById("chat-messages");
    const suggestionButtonsContainer = document.getElementById("suggestion-buttons");

    // Array de sugestões de perguntas
    const suggestions = [
        "Como criar usuário?",
        "Onde vejo os relatórios?",
        "Como alterar minha senha?",
        "O que é um KPI?"
    ];

    // Array de respostas do chatbot
    const responses = [
        { keywords: ["oi", "olá", "ola", "oi tudo bem", "e aí"], reply: ["Olá! Em que posso te ajudar hoje?", "Oi, tudo bem? Pode perguntar!", "Olá! O que você gostaria de saber?"] },
        { keywords: ["tchau", "até logo", "adeus", "obrigado"], reply: ["Fico feliz em ter ajudado! Se precisar de algo, é só me chamar.", "Até a próxima!", "Por nada! Volte sempre que precisar."] },
        { keywords: ["obrigado", "valeu", "show", "muito bom"], reply: "De nada! Fico feliz em ajudar." },
        { keywords: ["como criar usuário", "como adicionar usuario", "adicionar novo usuario"], reply: "Para criar um novo usuário, clique no botão ➕ **Novo Usuário** no topo da lista. Preencha os dados e salve." },
        { keywords: ["como editar usuario", "como alterar dados"], reply: "Para editar um usuário, localize-o na tabela e clique no botão de edição (geralmente um lápis ✏️)." },
        { keywords: ["como excluir usuario", "remover usuario", "apagar usuario"], reply: "Você pode remover um usuário clicando no botão 'Remover' (geralmente um ícone de lixeira 🗑️) na linha da tabela correspondente." },
        { keywords: ["como alterar senha", "mudar senha"], reply: "Se precisar alterar a senha, vá na tela de edição do seu usuário e preencha o campo de senha com uma nova combinação." },
        { keywords: ["permissoes de usuario", "acesso restrito", "acessos"], reply: "O sistema permite definir diferentes níveis de acesso e permissões para cada usuário, como administrador, analista ou visualizador. Verifique as configurações de cada usuário." },
        { keywords: ["como ver relatórios", "onde ver relatórios", "relatorios"], reply: "Para ver os relatórios, navegue até o menu **Dashboard**. Lá você pode usar os filtros para visualizar dados de chamados, como status, prioridade e analista." },
        { keywords: ["como criar chamado", "novo chamado"], reply: "A criação de novos chamados é feita através do botão 'Novo Chamado' na tela principal de chamados." },
        { keywords: ["o que é um kpi", "o que é kpi"], reply: "KPI significa Indicador-Chave de Desempenho (Key Performance Indicator). No Dashboard, os KPIs mostram as métricas mais importantes para a gestão de chamados, como tempo médio de resposta ou número de chamados fechados." },
        { keywords: ["email", "e-mail"], reply: "O e-mail do usuário é uma informação obrigatória para login e pode ser alterado na tela de edição do usuário." },
        { keywords: ["dashboard", "o que é dashboard"], reply: "O Dashboard é a tela principal de relatórios e métricas. Ele oferece uma visão geral do desempenho do seu sistema." },
        { keywords: ["lista", "tabela", "paginacao"], reply: "As tabelas de dados mostram as informações do sistema. Use a paginação na parte inferior para navegar entre as páginas de resultados." },
        { keywords: ["fallback"], reply: "Desculpe, não entendi. Por favor, tente perguntar de outra forma, como 'como criar usuário' ou 'o que é dashboard'." },

        // Saudações e despedidas (já existentes, mas reconfirmadas)
        { keywords: ["oi", "olá", "ola", "oi tudo bem", "e aí"], reply: ["Olá! Em que posso te ajudar hoje?", "Oi, tudo bem? Pode perguntar!", "Olá! O que você gostaria de saber?"] },
        { keywords: ["tchau", "até logo", "adeus", "obrigado"], reply: ["Fico feliz em ter ajudado! Se precisar de algo, é só me chamar.", "Até a próxima!", "Por nada! Volte sempre que precisar."] },
        { keywords: ["obrigado", "valeu", "show", "muito bom"], reply: "De nada! Fico feliz em ajudar." },

        // Usuários e Acesso
        { keywords: ["como criar usuario", "como adicionar usuario", "adicionar novo usuario"], reply: "Para criar um novo usuário, clique no botão ➕ **Novo Usuário** no topo da lista. Preencha os dados e salve." },
        { keywords: ["como editar usuario", "como alterar dados"], reply: "Para editar um usuário, localize-o na tabela e clique no botão de edição (geralmente um lápis ✏️)." },
        { keywords: ["como excluir usuario", "remover usuario", "apagar usuario"], reply: "Você pode remover um usuário clicando no botão 'Remover' (geralmente um ícone de lixeira 🗑️) na linha da tabela correspondente." },
        { keywords: ["como alterar senha", "mudar senha"], reply: "Se precisar alterar a senha, vá na tela de edição do seu usuário e preencha o campo de senha com uma nova combinação." },
        { keywords: ["permissoes de usuario", "acesso restrito", "acessos"], reply: "O sistema permite definir diferentes níveis de acesso e permissões para cada usuário, como administrador, analista ou visualizador. Verifique as configurações de cada usuário." },
        { keywords: ["perfil", "dados", "cadastro", "meu perfil"], reply: "Para visualizar ou editar seu perfil, clique no seu nome no canto superior direito da tela e selecione a opção 'Meu Perfil'." },
        { keywords: ["privilegio", "grupo de usuario"], reply: "Privilégios e grupos de usuários definem o que cada pessoa pode ou não fazer no sistema. Eles são configurados na tela de edição do usuário." },
        { keywords: ["login", "entrar no sistema"], reply: "Seu login é o seu e-mail. Para entrar no sistema, use seu e-mail e a sua senha." },
        { keywords: ["recuperar senha", "esqueci a senha"], reply: "Ainda não posso te ajudar a recuperar a senha, mas você pode pedir ao seu administrador para resetá-la para você." },
        { keywords: ["usuario bloqueado", "acesso bloqueado"], reply: "Se seu usuário estiver bloqueado, entre em contato com um administrador do sistema para que ele possa reativar sua conta." },
        { keywords: ["historico de usuario"], reply: "O histórico de um usuário mostra todas as ações que ele realizou. Você pode verificar essa informação na tela de detalhes do usuário." },
        { keywords: ["quem é o administrador", "contato do administrador"], reply: "O administrador é a pessoa com acesso total ao sistema. Verifique com sua equipe quem possui este cargo." },
        { keywords: ["usuario inativo"], reply: "Um usuário inativo não pode acessar o sistema. Esta opção é usada para desabilitar o acesso sem deletar a conta permanentemente." },
        { keywords: ["nome de usuario", "alterar nome"], reply: "Você pode alterar o nome do usuário na tela de edição. Basta preencher o novo nome e salvar." },
        { keywords: ["desativar conta"], reply: "Para desativar uma conta, acesse a tela de edição do usuário e mude o status para 'inativo'." },

        // Chamados e Tickets
        { keywords: ["abrir chamado", "novo chamado", "solicitacao", "novo ticket"], reply: "Para abrir um novo chamado, vá até a tela principal de chamados e clique no botão 'Novo Chamado'. Descreva o problema e clique em enviar." },
        { keywords: ["status do chamado", "chamado em andamento"], reply: "Você pode ver o status de qualquer chamado na tabela principal. Os status são: 'Em andamento', 'Pendente', 'Resolvido' e 'Fechado'." },
        { keywords: ["prioridade do chamado"], reply: "A prioridade indica a urgência do chamado. Os níveis são: 'Alta', 'Média' e 'Baixa'." },
        { keywords: ["como atribuir um chamado"], reply: "Para atribuir um chamado a um analista, selecione o chamado na lista e altere o campo 'Analista Responsável'." },
        { keywords: ["fila de chamados", "fila"], reply: "A fila de chamados é uma lista de todos os tickets que ainda não foram atribuídos a um analista. Você pode acessá-la na tela de chamados." },
        { keywords: ["o que é sla", "prazo do chamado"], reply: "SLA (Service Level Agreement) é o tempo máximo que um chamado tem para ser resolvido. Ele é definido de acordo com a prioridade do chamado." },
        { keywords: ["resolver problema", "solução do chamado"], reply: "Após encontrar a solução, você pode atualizar o chamado para o status 'Resolvido' e descrever os passos na seção de comentários." },
        { keywords: ["como fechar um chamado"], reply: "Um chamado é fechado após a solução ser confirmada. O analista pode fechar o chamado, ou o sistema pode fechá-lo automaticamente." },
        { keywords: ["anexar arquivo", "colocar anexo"], reply: "Para anexar arquivos, como prints ou documentos, use a opção 'Anexar' que fica dentro do chamado." },
        { keywords: ["comentario no chamado"], reply: "Os comentários permitem registrar as interações e o progresso do chamado. Você pode adicionar um novo comentário a qualquer momento." },
        { keywords: ["filtrar chamados"], reply: "Você pode filtrar os chamados por status, prioridade, analista responsável, ou data de criação. Os filtros ficam no topo da tela de chamados." },
        { keywords: ["buscar chamado"], reply: "Use a barra de busca no topo da tela para encontrar um chamado específico pelo seu ID ou título." },
        { keywords: ["tempo de resposta"], reply: "O tempo de resposta é a métrica que mede quanto tempo um analista leva para dar a primeira resposta após a abertura do chamado." },
        { keywords: ["chamados abertos"], reply: "A quantidade de chamados abertos indica a carga de trabalho atual da equipe. Você pode ver esta métrica no Dashboard." },

        // Relatórios e Dashboard
        { keywords: ["relatorio", "relatorios", "metrica", "kpi"], reply: "Os relatórios e métricas do sistema estão disponíveis no menu Dashboard. Você pode usar os filtros para encontrar as informações que precisa." },
        { keywords: ["graficos", "grafico"], reply: "O Dashboard exibe gráficos de barras, pizza e linhas para visualizar o volume de chamados por analista, status e prioridade." },
        { keywords: ["resumo de chamados"], reply: "O resumo de chamados é uma visão geral que mostra o número total de chamados abertos, resolvidos e fechados em um determinado período." },
        { keywords: ["tempo medio de atendimento", "tma"], reply: "O TMA (Tempo Médio de Atendimento) é uma métrica que indica a média de tempo que a equipe leva para resolver os chamados." },
        { keywords: ["relatorio por analista"], reply: "Você pode gerar relatórios de desempenho para cada analista, visualizando a quantidade de chamados que ele resolveu e o seu tempo médio de resposta." },
        { keywords: ["relatorio por status"], reply: "Este relatório mostra a distribuição de chamados por status, ajudando a identificar gargalos no processo." },
        { keywords: ["relatorio semanal", "relatorio mensal", "relatorio anual"], reply: "Você pode filtrar os relatórios por períodos de tempo como semana, mês ou ano para analisar tendências." },
        { keywords: ["exportar relatorio", "baixar relatorio"], reply: "Você pode exportar os relatórios em formatos como PDF e Excel através do botão 'Exportar' na tela do Dashboard." },
        { keywords: ["tendencias"], reply: "Analisar tendências de chamados ajuda a prever a demanda e alocar recursos de forma mais eficiente." },
        { keywords: ["volume de chamados"], reply: "O volume de chamados mostra a quantidade de solicitações que o sistema recebeu em um determinado período. É um KPI fundamental." },
        { keywords: ["analise de dados"], reply: "A análise de dados do Dashboard ajuda a tomar decisões estratégicas para melhorar a eficiência do seu fluxo de trabalho." },

        // Sistema e Funcionalidades Gerais
        { keywords: ["sistema", "plataforma"], reply: "A plataforma InovaTech é uma solução completa para a gestão de chamados e relacionamento com o cliente." },
        { keywords: ["navegacao", "menu"], reply: "O menu de navegação está no lado esquerdo da tela. Clique em cada ícone para acessar as diferentes funcionalidades do sistema." },
        { keywords: ["pagina inicial"], reply: "A página inicial é o Dashboard. Ela oferece uma visão geral do status da sua operação." },
        { keywords: ["configuracoes", "configurar"], reply: "As configurações do sistema permitem personalizar fluxos de trabalho, notificações e outras funcionalidades. Apenas o administrador tem acesso a essa tela." },
        { keywords: ["ajuda", "suporte", "duvidas"], reply: "Se você não encontrar a resposta que procura, pode entrar em contato com o suporte técnico através do botão de ajuda no menu." },
        { keywords: ["manual do usuario", "tutorial"], reply: "O manual do usuário está disponível no menu de ajuda e contém tutoriais detalhados sobre todas as funcionalidades do sistema." },
        { keywords: ["guia de inicio rapido", "guia rapido"], reply: "O guia de início rápido te ajuda a dar os primeiros passos no sistema, desde o login até a criação de seu primeiro chamado." },
        { keywords: ["atalhos do teclado"], reply: "O sistema possui atalhos de teclado para agilizar o trabalho. Por exemplo, 'Ctrl + N' pode abrir um novo chamado." },
        { keywords: ["versao do sistema", "atualizacao"], reply: "Para ver a versão atual do sistema, vá até a tela 'Sobre'. As atualizações são lançadas regularmente com novas funcionalidades e melhorias." },
        { keywords: ["novidades"], reply: "As novidades sobre a plataforma e as próximas atualizações são comunicadas no blog oficial da InovaTech." },
        { keywords: ["email", "e-mail"], reply: "O e-mail do usuário é uma informação obrigatória para login e pode ser alterado na tela de edição do usuário." },
        { keywords: ["fallback"], reply: "Desculpe, não entendi. Por favor, tente perguntar de outra forma, como 'como criar usuário' ou 'o que é dashboard'." },
        { keywords: ["como alterar senha", "mudar senha", "alterar minha senha"], reply: "Para alterar sua senha, vá para 'Meu Perfil' no menu de usuário. Lá, você encontrará a opção para criar uma nova senha. Lembre-se de usar uma senha forte, com letras, números e símbolos." },
        { keywords: ["esqueci a senha", "redefinir senha", "recuperar senha", "perdi a senha"], reply: "Se você esqueceu sua senha, o administrador do sistema pode redefinir uma nova para você. Por motivos de segurança, eu não posso fazer isso diretamente. Por favor, entre em contato com sua equipe de TI ou o gestor responsável." },
        { keywords: ["minha senha não funciona", "erro de senha", "senha invalida"], reply: "Verifique se a senha foi digitada corretamente, incluindo letras maiúsculas e minúsculas. Se o problema persistir, pode ser necessário redefinir sua senha com a ajuda de um administrador." },
        { keywords: ["senha segura", "como criar senha forte", "melhor senha"], reply: "Uma senha segura deve ter no mínimo 8 caracteres, com uma combinação de letras maiúsculas, minúsculas, números e símbolos (!@#$). Evite usar datas de nascimento ou nomes fáceis de adivinhar." },
        { keywords: ["validade da senha", "senha expirou"], reply: "A sua senha tem um prazo de validade por questões de segurança. Você receberá uma notificação quando ela estiver perto de expirar. Fique atento para alterá-la antes do prazo." },
        { keywords: ["login e senha", "acesso"], reply: "Seu login é o seu e-mail de cadastro. Use-o junto com sua senha para acessar o sistema. Se tiver problemas, verifique se seu usuário não está inativo ou bloqueado." },
        { keywords: ["quantas vezes posso errar a senha", "senha bloqueada"], reply: "Após um certo número de tentativas de senha incorretas, sua conta será bloqueada temporariamente por segurança. Isso impede acessos não autorizados." },
        { keywords: ["notificação de senha", "alerta de senha"], reply: "O sistema envia alertas quando sua senha precisa ser alterada ou quando há tentativas de login suspeitas. Sempre verifique seu e-mail para esses avisos." },
        { keywords: ["senha temporaria"], reply: "Uma senha temporária é geralmente fornecida quando sua senha é redefinida por um administrador. Você deve usá-la apenas para o primeiro login e alterá-la imediatamente para uma senha de sua preferência." },
        { keywords: ["posso usar a mesma senha de outros sites"], reply: "Para sua segurança, **não é recomendado** usar a mesma senha em sites diferentes. Crie senhas únicas para cada sistema para evitar que, se uma for comprometida, as outras também sejam." }
        ];

    // Função para normalizar texto (sem acentos e em minúsculas)
    function normalizeText(text) {
        return text.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    // Função para adicionar uma nova mensagem na tela
    function addMessage(sender, text) {
        const msg = document.createElement("div");
        msg.className = `chat-message ${sender}`;
        msg.innerHTML = text;
        chatMessages.appendChild(msg);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Função para renderizar os botões de sugestão
    function renderSuggestions() {
        suggestionButtonsContainer.innerHTML = ''; // Limpa botões antigos
        suggestions.forEach(text => {
            const button = document.createElement("button");
            button.className = "suggestion-btn";
            button.innerText = text;
            button.addEventListener("click", () => {
                userInput.value = text;
                sendBtn.click(); // Simula o clique no botão Enviar
            });
            suggestionButtonsContainer.appendChild(button);
        });
    }

    // Função para encontrar a resposta do bot
    function getBotResponse(message) {
        const normalizedMessage = normalizeText(message);
        for (const item of responses) {
            for (const keyword of item.keywords) {
                if (normalizedMessage.includes(keyword)) {
                    if (Array.isArray(item.reply)) {
                        const randomIndex = Math.floor(Math.random() * item.reply.length);
                        return item.reply[randomIndex];
                    }
                    return item.reply;
                }
            }
        }
        return responses.find(item => item.keywords.includes("fallback")).reply;
    }

    // Mensagem inicial do bot + Sugestões
    setTimeout(() => {
        addMessage("bot", "Olá! Eu sou a Inteligência Artificial da InovaTech. Posso te ajudar com dúvidas sobre usuários, relatórios, chamados e muito mais.");
        renderSuggestions(); // Exibe os botões de sugestão
    }, 500);

    // Função para adicionar uma nova mensagem na tela com data e hora
    function addMessage(sender, text) {
        const now = new Date();
        const formattedTime = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const formattedDate = now.toLocaleDateString('pt-BR');

        const msg = document.createElement("div");
        msg.className = `chat-message ${sender}`;
        
        // Cria o conteúdo da mensagem com a data e hora
        const messageText = document.createElement("span");
        messageText.innerHTML = text;

        const messageInfo = document.createElement("span");
        messageInfo.className = "chat-message-info";
        messageInfo.innerText = `${formattedTime} · ${formattedDate}`;

        msg.appendChild(messageText);
        msg.appendChild(messageInfo);
        
        chatMessages.appendChild(msg);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Ação ao enviar o formulário
    chatForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;

        addMessage("user", message);
        userInput.value = "";
        suggestionButtonsContainer.innerHTML = ''; // Esconde os botões ao enviar

        const typingIndicator = document.createElement("div");
        typingIndicator.className = "chat-message bot typing-indicator";
        chatMessages.appendChild(typingIndicator);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        setTimeout(() => {
            typingIndicator.remove();
            const botReply = getBotResponse(message);
            addMessage("bot", botReply);
        }, 800);
    });
});