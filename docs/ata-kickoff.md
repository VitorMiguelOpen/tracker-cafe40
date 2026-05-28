# Ata de reunião — Kickoff (briefing com cliente)

| Campo                  | Valor                                   |
| ---------------------- | --------------------------------------- |
| Data                   | 20/05/2026                              |
| Tipo                   | Kickoff — briefing com cliente          |
| Cliente (stakeholder)  | Sandro Ferreira                         |
| Responsável de Produto | Ana Clara Silva                         |
| Tecnologia citada      | MQTT (protocolo de mensageria do sensor)|

## Contexto

O cliente possui um equipamento com sensor IoT acoplado que coleta o status (ligado/desligado) via MQTT. Apesar de o sensor existir e gerar dados, o cliente **não tem acesso a nenhuma visualização ou análise** dessas informações.

## Dores relatadas pelo cliente

- Não sabe quando o equipamento está ligado ou desligado
- Não tem métricas de consumo por horário ou por dia
- Não consegue prever horários de maior demanda
- Não possui histórico nem análise de comportamento do equipamento
- Não tem manual nem material de treinamento para novos colaboradores

> _"Eu estou cego quanto ao uso desse equipamento."_

## Objetivo da solução

Desenvolver um dashboard visual moderno que transforme os dados brutos (ligado/desligado) em informações inteligentes e úteis para a tomada de decisão, acessível aos colaboradores e com boa experiência de uso.

## Funcionalidades esperadas

**Must have**
- Quantidade de itens consumidos no dia
- Gráfico de consumo por hora
- Gráfico diário e semanal
- Identificação de horário de pico
- Indicador visual de status (mudança visual clara)
- Indicadores de tendência e gráficos de médias
- Atualização em tempo real

**Nice to have**
- Dashboard responsivo (celular, tablet, TV, monitor)
- Alertas e notificações de consumo
- Estimativa de consumo de energia
- Previsão de pico (modelo preditivo)
- Integração com TV/monitor da empresa
- Vídeo demonstrativo do equipamento (onboarding) e da solução

## Mensagem final do cliente

> _"Pensar além da cafeteira. O desafio aqui é sobre dados, experiência e criatividade. Transformar dados simples de ligado/desligado em algo que gere valor real — que qualquer empresa colocaria na sua fábrica."_
