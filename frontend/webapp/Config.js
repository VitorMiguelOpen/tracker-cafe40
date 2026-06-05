sap.ui.define([], function () {
    "use strict";

    // Endereço do backend .NET.
    //
    // Caso normal (o backend serve o próprio dashboard): usamos a MESMA ORIGEM
    // da página. Assim funciona tanto em http://localhost:5000 quanto quando
    // hospedado num servidor (ex.: http://servidor:5000) — qualquer máquina que
    // abrir a URL chama a API no mesmo host, sem configuração.
    //
    // Modo "ui5 serve" (desenvolvimento do front com live-reload, porta 8081):
    // o front roda numa porta separada, então apontamos para a API em :5000.
    var sIsUi5Serve = window.location.port === "8081";
    var sBackendUrl = sIsUi5Serve ? "http://localhost:5000" : window.location.origin;

    return {
        backendUrl: sBackendUrl,
        hubPath: "/hubs/status",
        machineCode: "SAACE"
    };
});
