var Tenant = /** @class */ (function () {
    function Tenant() {
        this.tenantId = ko.observable("");
        this.name = ko.observable("");
        this.tenantCode = ko.observable("");
        this.enabled = ko.observable(false);
    }
    Tenant.prototype.Load = function (item) {
        if (item != null) {
            this.tenantId(item.tenantId);
            this.name(item.name);
            this.tenantCode(item.tenantCode);
            this.enabled(item.enabled);
        }
    };
    return Tenant;
}());
var Source = /** @class */ (function () {
    function Source() {
        this.sourceId = ko.observable("");
        this.sourceName = ko.observable("");
        this.sourceType = ko.observable("");
        this.sourceCategory = ko.observable("");
        this.sourceTemplate = ko.observable("");
        this.tenantId = ko.observable("");
        this.tenant = ko.observable(null);
        this.tenant(new Tenant());
    }
    Source.prototype.Load = function (item) {
        if (item != null) {
            this.sourceId(item.sourceId);
            this.sourceName(item.sourceName);
            this.sourceType(item.sourceType);
            this.sourceCategory(item.sourceCategory);
            this.sourceTemplate(item.sourceTemplate);
            this.tenantId(item.tenantId);
            if (item.tenant != null) {
                var newTenant = new Tenant();
                newTenant.Load(item.tenant);
                this.tenant(newTenant);
            }
        }
    };
    return Source;
}());
//# sourceMappingURL=Utilities.js.map