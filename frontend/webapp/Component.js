sap.ui.define([
    "sap/ui/core/UIComponent"
], function (UIComponent) {
    "use strict";

    return UIComponent.extend("cafetracker.Component", {
        metadata: {
            manifest: "json"
        },

        init: function () {
            // Inicialização padrão (lê o manifest, monta a rootView etc.).
            UIComponent.prototype.init.apply(this, arguments);
        }
    });
});
