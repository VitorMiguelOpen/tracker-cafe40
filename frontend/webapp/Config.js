sap.ui.define([], function () {
    "use strict";

    // Único lugar para apontar o endereço do backend .NET.
    // O backend roda em http://localhost:5000 (ver launchSettings.json / .env).
    return {
        backendUrl: "http://localhost:5000",
        hubPath: "/hubs/status",
        machineCode: "SAACE"
    };
});
