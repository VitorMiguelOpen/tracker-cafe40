sap.ui.define([], function () {
    "use strict";

    return {
        /** Segundos → texto amigável: "2h 05m", "30 min" ou "—". */
        durationLabel: function (seconds) {
            seconds = parseInt(seconds, 10);
            if (isNaN(seconds) || seconds <= 0) {
                return "—";
            }
            var h = Math.floor(seconds / 3600);
            var m = Math.floor((seconds % 3600) / 60);
            if (h > 0) {
                return h + "h " + (m < 10 ? "0" + m : m) + "m";
            }
            if (m > 0) {
                return m + " min";
            }
            return seconds + " s";
        },

        /** Hora (0..23) → "08h". null → "—". */
        hourLabel: function (hour) {
            if (hour === null || hour === undefined || hour === "") {
                return "—";
            }
            hour = parseInt(hour, 10);
            return (hour < 10 ? "0" + hour : "" + hour) + "h";
        },

        /** Estado do indicador de status: 1 → Success (verde), 0 → Error (vermelho). */
        statusState: function (value) {
            return parseInt(value, 10) === 1 ? "Success" : "Error";
        },

        /** Ícone do status: 1 → "ligado", 0 → "desligado". */
        statusIcon: function (value) {
            return parseInt(value, 10) === 1
                ? "sap-icon://connected"
                : "sap-icon://disconnected";
        },

        /** Estado visual da tendência: Aumentando → Success, Diminuindo → Error, Estável → Information. */
        trendState: function (trend) {
            switch (trend) {
                case "Aumentando": return "Success";
                case "Diminuindo": return "Error";
                default: return "Information";
            }
        },

        /** Ícone de seta para a tendência. */
        trendIcon: function (trend) {
            switch (trend) {
                case "Aumentando": return "sap-icon://trend-up";
                case "Diminuindo": return "sap-icon://trend-down";
                default: return "sap-icon://horizontal-bar-chart";
            }
        },

        /** Indicador de conexão em tempo real. */
        connectionText: function (connected) {
            return connected ? "Tempo real conectado" : "Tempo real desconectado";
        },

        connectionState: function (connected) {
            return connected ? "Success" : "Warning";
        }
    };
});
