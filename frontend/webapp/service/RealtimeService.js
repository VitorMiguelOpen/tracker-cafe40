sap.ui.define([
    "sap/ui/base/Object",
    "cafetracker/Config",
    "sap/base/Log"
], function (BaseObject, Config, Log) {
    "use strict";

    /**
     * Envelopa o cliente SignalR (window.signalR, carregado via CDN no index.html).
     * Conecta no hub do backend e dispara callbacks quando o status muda ou quando
     * o estado da conexão muda. Tem reconexão automática embutida.
     */
    return BaseObject.extend("cafetracker.service.RealtimeService", {

        constructor: function () {
            BaseObject.call(this);
            this._connection = null;
        },

        /**
         * @param {function} onStatusChanged  recebe o payload { machineCode, value, state, label, eventTime }
         * @param {function} onConnectionChanged recebe um boolean (conectado?)
         */
        start: function (onStatusChanged, onConnectionChanged) {
            if (!window.signalR) {
                Log.error("Cliente SignalR não carregou (verifique a CDN no index.html).");
                onConnectionChanged(false);
                return;
            }

            var sUrl = Config.backendUrl + Config.hubPath;

            this._connection = new window.signalR.HubConnectionBuilder()
                .withUrl(sUrl)
                .withAutomaticReconnect()
                .configureLogging(window.signalR.LogLevel.Warning)
                .build();

            // Evento empurrado pelo servidor a cada mudança de status.
            this._connection.on("StatusChanged", function (payload) {
                onStatusChanged(payload);
            });

            // Acompanha o ciclo de vida da conexão (para o indicador visual).
            this._connection.onreconnecting(function () { onConnectionChanged(false); });
            this._connection.onreconnected(function () { onConnectionChanged(true); });
            this._connection.onclose(function () { onConnectionChanged(false); });

            this._connection.start()
                .then(function () {
                    Log.info("SignalR conectado em " + sUrl);
                    onConnectionChanged(true);
                })
                .catch(function (err) {
                    Log.error("Falha ao conectar no SignalR: " + err);
                    onConnectionChanged(false);
                });
        },

        stop: function () {
            if (this._connection) {
                this._connection.stop();
                this._connection = null;
            }
        }
    });
});
