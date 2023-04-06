//#region DataObjects
/**
 * A class used to return a response from the sever.
 */
var tsutilsActionInfo = /** @class */ (function () {
    function tsutilsActionInfo() {
        /**
         * Indicates success or failure from the server.
         */
        this.Result = ko.observable(false);
        /**
         * An array of strings containing any response messages.
         */
        this.Messages = ko.observableArray([]);
        /**
         * The date of the response.
         */
        this.Date = ko.observable(null);
        /**
         * An updated JSON Web Token included with some responses.
         */
        this.Token = ko.observable("");
    }
    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.ActionInfo} data - A DTO object containing data.
     */
    tsutilsActionInfo.prototype.Load = function (data) {
        if (data != null) {
            this.Result(data.Result);
            this.Date(data.Date);
            var t = this;
            this.Messages(data.Messages);
            this.Token(data.Token);
        }
    };
    return tsutilsActionInfo;
}());
/**
 * A class used for performing AJAX lookups.
 */
var tsutilsAjaxLookup = /** @class */ (function () {
    function tsutilsAjaxLookup() {
        /**
         * The response from the server.
         */
        this.ActionInfo = ko.observable(new tsutilsActionInfo);
        /**
         * The search text for performing the AJAX lookup.
         */
        this.Search = ko.observable("");
        /**
         * An optional collection of parameters.
         */
        this.Parameters = ko.observableArray([]);
        /**
         * The AjaxResults from the server.
         */
        this.Results = ko.observableArray([]);
    }
    /**
     * Loads data into the element from a DTO.
     * @param {server.AjaxLookup} data - A DTO object containing data.
     */
    tsutilsAjaxLookup.prototype.Load = function (data) {
        if (data != null) {
            this.ActionInfo().Load(data.ActionInfo);
            this.Search(data.Search);
            this.Results([]);
            var parameters_1 = [];
            if (data.Parameters != null) {
                data.Parameters.forEach(function (element) {
                    parameters_1.push(element);
                });
            }
            this.Parameters(parameters_1);
            if (data.Results != null) {
                var results_1 = [];
                data.Results.forEach(function (element) {
                    var item = new tsutilsAjaxResults();
                    item.Load(element);
                    results_1.push(item);
                });
                this.Results(results_1);
            }
        }
    };
    return tsutilsAjaxLookup;
}());
/**
 * A class used for AJAX result data.
 */
var tsutilsAjaxResults = /** @class */ (function () {
    function tsutilsAjaxResults() {
        /**
         * The label to display with this result.
         */
        this.label = ko.observable("");
        /**
         * The value of this result.
         */
        this.value = ko.observable("");
    }
    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.AjaxResults} data - A DTO object containing data.
     */
    tsutilsAjaxResults.prototype.Load = function (data) {
        if (data != null) {
            this.label(data.label);
            this.value(data.value);
        }
    };
    return tsutilsAjaxResults;
}());
/**
 * A classed used for a simple response with messages from the server.
 */
var tsutilsBooleanResponse = /** @class */ (function () {
    function tsutilsBooleanResponse() {
        /**
         * The response from the server indicating success or failure.
         */
        this.Response = ko.observable(false);
        /**
         * An array of messages returned from the server.
         */
        this.Messages = ko.observableArray([]);
    }
    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.BooleanResponse} data - A DTO object containing data.
     */
    tsutilsBooleanResponse.prototype.Load = function (data) {
        if (data != null) {
            this.Response(data.Response);
            this.Messages(data.Messages);
        }
    };
    return tsutilsBooleanResponse;
}());
//#endregion
//# sourceMappingURL=utilModels.js.map