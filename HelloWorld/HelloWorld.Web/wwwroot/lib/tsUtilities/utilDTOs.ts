/**
 * DTOs (Data Transfer Objects) used to return Web Api results.
 */
declare module tsutilsDTO {
    /**
     * The response from the server.
     */
    interface tsutilsActionInfo {
        /**
         * Indicates success or failure from the server.
         */
        Result: boolean;
        /**
         * An array of strings containing any response messages.
         */
        Messages: string[];
        /**
         * The date of the response.
         */
        Date: Date;
        /**
         * An updated JSON Web Token included with some responses.
         */
        Token: string;
    }

    /**
     * A simple key pair value object
     */
    interface tsutilsKeyPairValues {
        /**
         * The key
         */
        key: string;
        /**
         * The value for the key
         */
        value: string;
    }

    /**
     * An objects used to perform AJAX lookups
     */
    interface tsutilsAjaxLookup {
        /**
         * The response from the server.
         */
        ActionInfo: tsutilsActionInfo;
        /**
         * The search text for performing the AJAX lookup.
         */
        Search: string;
        /**
         * An optional collection of parameters.
         */
        Parameters: string[];
        /**
         * The AjaxResults from the server.
         */
        Results: tsutilsAjaxResults[];
    }

    /**
     * An indivual Ajax result item
     */
    interface tsutilsAjaxResults {
        /**
         * The label to display with this result.
         */
        label: string;
        /**
         * The value of this result.
         */
        value: string;
    }

    /**
     * A simple response with messages
     */
    interface tsutilsBooleanResponse {
        /**
         * The response from the server indicating success or failure.
         */
        Response: boolean;
        /**
         * An array of messages returned from the server.
         */
        Messages: string[];
    }

    /**
     * An object used to pass buttons to the Modal dialogs and messages.
     */
    interface tsutilsDialogButton {
        /**
         * A unique Id for this button. If none is provided a GUID will be created and assigned as the Id.
         */
        Id?: string;
        /**
         * The text to display on the button
         */
        Text?: string;
        /**
         * An icon class to include in an <i> element on the button. Set to "auto" to attempt to automatically select an icon based on the button text.
         */
        Icon?: string;
        /**
         * The class used for the button (defaults to 'btn btn-secondary').
         */
        Class?: string;
        /**
         * An optional callback handler method to run when the button is clicked.
         */
        CallbackHandler?: Function;
        /**
         * Indicates if this button causes the dialog to close after clicked. Defaults to true.
         */
        ClosesDialog?: boolean;
        /**
         * Indicates the position of this button. The options are "top", "top-fixed", and "bottom" with a default of "bottom". Using "top-fixed" causes the buttons to float with the content.
         */
        Position?: string;
        /**
         * An optional group name to group this button with others. Buttons with the same group name will be put in a button group.
         */
        GroupName?: string;
    }

    interface tsutilsConfirmButton {
        /**
         * A unique Id for this button. If none is provided a GUID will be created and assigned as the Id.
         */
        Id?: string;
        /**
         * The text to display on the button
         */
        Text?: string;
        /**
         * An icon class to include in an <i> element on the button. Set to "auto" to attempt to automatically select an icon based on the button text.
         */
        Icon?: string;
        /**
         * The class used for the button (defaults to 'btn btn-secondary').
         */
        Class?: string;
        /**
         * An optional callback handler method to run when the button is clicked.
         */
        CallbackHandler?: Function;
        /**
         * Indicates if this button causes the confirmation message to close after clicked. Defaults to true.
         */
        ClosesConfirmation?: boolean;
    }
}