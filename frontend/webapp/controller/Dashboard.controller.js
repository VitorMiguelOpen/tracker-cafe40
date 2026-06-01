sap.ui.define([
    "sap/ui/core/mvc/Controller",
    "sap/ui/model/json/JSONModel",
    "cafetracker/model/formatter",
    "cafetracker/service/RealtimeService",
    "cafetracker/Config",
    "sap/m/MessageToast",
    "sap/base/Log"
], function (Controller, JSONModel, formatter, RealtimeService, Config, MessageToast, Log) {
    "use strict";

    var WEEKDAYS = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

    return Controller.extend("cafetracker.controller.Dashboard", {

        formatter: formatter,

        onInit: function () {
            // Modelo padrão (não-nomeado) com toda a estrutura do dashboard.
            this._model = new JSONModel({
                connected: false,
                selectedDate: this._today(),
                view: "Diario",
                status: { value: 0, label: "Desligado", state: "Stopped", eventTimeLabel: "—" },
                activationsToday: 0,
                peak: { hour: null, totalSeconds: 0 },
                trend: { trend: "Estável", averageSecondsPerDay: 0, averageLabel: "—" },
                daily: { activations: 0, totalSeconds: 0 },
                hourly: [],
                weekly: []
            });
            this.getView().setModel(this._model);

            // Embeleza os gráficos quando estiverem renderizados.
            this.getView().addEventDelegate({
                onAfterRendering: this._styleCharts.bind(this)
            });

            // Liga o tempo real e carrega os dados iniciais.
            this._realtime = new RealtimeService();
            this._realtime.start(this._onStatusChanged.bind(this), this._onConnectionChanged.bind(this));
            this._loadAll();
        },

        onExit: function () {
            if (this._realtime) {
                this._realtime.stop();
            }
        },

        // ===================== Tempo real (SignalR) =====================

        _onStatusChanged: function (payload) {
            // payload: { machineCode, value, state, label, eventTime }
            this._model.setProperty("/status", {
                value: payload.value,
                label: payload.label,
                state: payload.state,
                eventTimeLabel: this._dateTimeLabel(payload.eventTime)
            });

            // O status mudou → atualiza os números do dia atual.
            this._loadActivations();
            if (this._model.getProperty("/selectedDate") === this._today()) {
                this._loadDayData(this._today());
            }
        },

        _onConnectionChanged: function (connected) {
            this._model.setProperty("/connected", connected);
        },

        // ===================== Navegação =====================

        onDateChange: function () {
            this._loadDayData(this._model.getProperty("/selectedDate"));
        },

        onPrevDay: function () {
            this._shiftDay(-1);
        },

        onNextDay: function () {
            this._shiftDay(1);
        },

        onViewChange: function () {
            if (this._model.getProperty("/view") === "Semanal") {
                this._loadWeekly(this._model.getProperty("/selectedDate"));
            }
        },

        _shiftDay: function (delta) {
            var current = this._parseDate(this._model.getProperty("/selectedDate"));
            current.setDate(current.getDate() + delta);
            this._model.setProperty("/selectedDate", this._formatDate(current));
            this._loadDayData(this._formatDate(current));
        },

        // ===================== Carga de dados (REST) =====================

        _loadAll: function () {
            this._loadStatus();
            this._loadActivations();
            this._loadTrend();
            this._loadDayData(this._model.getProperty("/selectedDate"));
        },

        _loadDayData: function (date) {
            this._loadHourly(date);
            this._loadDaily(date);
            this._loadPeak(date);
            if (this._model.getProperty("/view") === "Semanal") {
                this._loadWeekly(date);
            }
        },

        _loadStatus: function () {
            this._get("/api/status/current", function (data) {
                this._model.setProperty("/status", {
                    value: data.value,
                    label: data.label,
                    state: data.state,
                    eventTimeLabel: this._dateTimeLabel(data.eventTime)
                });
            });
        },

        _loadActivations: function () {
            this._get("/api/consumption/activations/today", function (data) {
                this._model.setProperty("/activationsToday", data.activations);
            });
        },

        _loadTrend: function () {
            this._get("/api/consumption/trend?days=7", function (data) {
                this._model.setProperty("/trend", {
                    trend: data.trend,
                    averageSecondsPerDay: data.averageSecondsPerDay,
                    averageLabel: formatter.durationLabel(data.averageSecondsPerDay)
                });
            });
        },

        _loadHourly: function (date) {
            this._get("/api/consumption/hourly?date=" + date, function (rows) {
                var mapped = rows.map(function (r) {
                    return {
                        hour: r.hour,
                        hourLabel: (r.hour < 10 ? "0" + r.hour : "" + r.hour) + "h",
                        activations: r.activations,
                        totalSeconds: r.totalSeconds
                    };
                });
                this._model.setProperty("/hourly", mapped);
            });
        },

        _loadDaily: function (date) {
            this._get("/api/consumption/daily?date=" + date, function (data) {
                this._model.setProperty("/daily", {
                    activations: data.activations,
                    totalSeconds: data.totalSeconds
                });
            });
        },

        _loadPeak: function (date) {
            this._get("/api/consumption/peak?date=" + date, function (data) {
                this._model.setProperty("/peak", {
                    hour: data.hour,
                    totalSeconds: data.totalSeconds
                });
            });
        },

        _loadWeekly: function (date) {
            this._get("/api/consumption/weekly?date=" + date, function (rows) {
                var mapped = rows.map(function (r) {
                    var d = this._parseDate(r.date);
                    return {
                        dayLabel: WEEKDAYS[d.getDay()] + " " + this._formatDayMonth(d),
                        minutes: Math.round(r.totalSeconds / 60),
                        activations: r.activations
                    };
                }.bind(this));
                this._model.setProperty("/weekly", mapped);
            });
        },

        // ===================== Auxiliares =====================

        /**
         * GET na API. Em caso de sucesso, chama onData(json) com o "this" do
         * controller. Em caso de erro, trata num único ponto (log + toast) e
         * NÃO propaga a rejeição (evita "Uncaught (in promise)" e impede que o
         * handler de sucesso rode sem dados).
         */
        _get: function (path, onData) {
            return fetch(Config.backendUrl + path)
                .then(function (res) {
                    if (!res.ok) {
                        throw new Error("HTTP " + res.status);
                    }
                    return res.json();
                })
                .then(onData.bind(this))
                .catch(function (err) {
                    Log.error("Falha ao chamar " + path + ": " + err);
                    MessageToast.show("Não foi possível carregar dados do backend.");
                }.bind(this));
        },

        /** Aplica estilo aos VizFrames (esconde legenda, dá título ao eixo). */
        _styleCharts: function () {
            if (this._chartsStyled) {
                return;
            }
            var oView = this.getView();
            ["hourlyChart", "weeklyChart"].forEach(function (sId) {
                var oChart = oView.byId(sId);
                if (oChart && oChart.setVizProperties) {
                    oChart.setVizProperties({
                        legend: { visible: false },
                        title: { visible: false },
                        plotArea: { dataLabel: { visible: true } }
                    });
                }
            });
            this._chartsStyled = true;
        },

        _today: function () {
            return this._formatDate(new Date());
        },

        _parseDate: function (sDate) {
            // sDate no formato "yyyy-MM-dd" → Date local (meia-noite).
            return new Date(sDate + "T00:00:00");
        },

        _formatDate: function (oDate) {
            var y = oDate.getFullYear();
            var m = oDate.getMonth() + 1;
            var d = oDate.getDate();
            return y + "-" + (m < 10 ? "0" + m : m) + "-" + (d < 10 ? "0" + d : d);
        },

        _formatDayMonth: function (oDate) {
            var d = oDate.getDate();
            var m = oDate.getMonth() + 1;
            return (d < 10 ? "0" + d : d) + "/" + (m < 10 ? "0" + m : m);
        },

        _dateTimeLabel: function (sIso) {
            if (!sIso) {
                return "—";
            }
            try {
                return new Date(sIso).toLocaleString("pt-BR");
            } catch (e) {
                return sIso;
            }
        }
    });
});
