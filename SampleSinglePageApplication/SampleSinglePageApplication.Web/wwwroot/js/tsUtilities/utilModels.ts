//#region DataObjects
/**
 * A class used to return a response from the sever.
 */
class tsutilsActionInfo {
    /**
     * Indicates success or failure from the server.
     */
    Result: KnockoutObservable<boolean> = ko.observable(false);
    /**
     * An array of strings containing any response messages.
     */
    Messages: KnockoutObservableArray<string> = ko.observableArray([]);
    /**
     * The date of the response.
     */
    Date: KnockoutObservable<Date> = ko.observable(null);
    /**
     * An updated JSON Web Token included with some responses.
     */
    Token: KnockoutObservable<string> = ko.observable("");

    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.ActionInfo} data - A DTO object containing data.
     */
    Load(data: tsutilsDTO.tsutilsActionInfo) {
        if (data != null) {
            this.Result(data.Result);
            this.Date(data.Date);
            var t = this;
            this.Messages(data.Messages);
            this.Token(data.Token);
        }
    }
}

/**
 * A class used for performing AJAX lookups.
 */
class tsutilsAjaxLookup {
    /**
     * The response from the server.
     */
    ActionInfo: KnockoutObservable<tsutilsActionInfo> = ko.observable(new tsutilsActionInfo);
    /**
     * The search text for performing the AJAX lookup.
     */
    Search: KnockoutObservable<string> = ko.observable("");
    /**
     * An optional collection of parameters.
     */
    Parameters: KnockoutObservableArray<string> = ko.observableArray([]);
    /**
     * The AjaxResults from the server.
     */
    Results: KnockoutObservableArray<tsutilsAjaxResults> = ko.observableArray([]);

    /**
     * Loads data into the element from a DTO.
     * @param {server.AjaxLookup} data - A DTO object containing data.
     */
    Load(data: tsutilsDTO.tsutilsAjaxLookup) {
        if (data != null) {
            this.ActionInfo().Load(data.ActionInfo);
            this.Search(data.Search);
            this.Results([]);
            let parameters: string[] = [];
            if (data.Parameters != null) {
                data.Parameters.forEach(function (element) {
                    parameters.push(element);
                });
            }
            this.Parameters(parameters);
            if (data.Results != null) {
                let results: tsutilsAjaxResults[] = [];
                data.Results.forEach(function (element) {
                    let item: tsutilsAjaxResults = new tsutilsAjaxResults();
                    item.Load(element);
                    results.push(item);
                });
                this.Results(results);
            }
        }
    }
}

/**
 * A class used for AJAX result data.
 */
class tsutilsAjaxResults {
    /**
     * The label to display with this result.
     */
    label: KnockoutObservable<string> = ko.observable("");
    /**
     * The value of this result.
     */
    value: KnockoutObservable<string> = ko.observable("");

    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.AjaxResults} data - A DTO object containing data.
     */
    Load(data: tsutilsDTO.tsutilsAjaxResults) {
        if (data != null) {
            this.label(data.label);
            this.value(data.value);
        }
    }
}

/**
 * A classed used for a simple response with messages from the server.
 */
class tsutilsBooleanResponse {
    /**
     * The response from the server indicating success or failure.
     */
    Response: KnockoutObservable<boolean> = ko.observable(false);
    /**
     * An array of messages returned from the server.
     */
    Messages: KnockoutObservableArray<string> = ko.observableArray([]);

    /**
     * Loads data into the element from a DTO.
     * @param {tsutilsDTO.BooleanResponse} data - A DTO object containing data.
     */
    Load(data: tsutilsDTO.tsutilsBooleanResponse) {
        if (data != null) {
            this.Response(data.Response);
            this.Messages(data.Messages);
        }
    }
}
//#endregion