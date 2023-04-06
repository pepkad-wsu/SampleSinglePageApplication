declare module server {
    enum settingType {
        boolean = 0,
        dateTime = 1,
        encryptedText = 2,
        guid = 3,
        numberDecimal = 4,
        numberDouble = 5,
        numberInt = 6,
        object = 7,
        text = 8
    }

    interface actionResponseObject {
        actionResponse: booleanResponse;
    }

    interface addModule {
        module: string;
        name: string;
    }

    interface ajaxLookup extends actionResponseObject {
        tenantId: string;
        search: string;
        parameters: string[];
        results: ajaxResults[];
    }

    interface ajaxResults {
        label: string;
        value: string;
        email: string;
        username: string;
        extra1: string;
        extra2: string;
        extra3: string;
    }

    interface applicationSettings extends actionResponseObject {
        applicationURL: string;
        defaultTenantCode: string;
        encryptionKey: string;
        mailServer: string;
        mailServerPassword: string;
        mailServerPort: number;
        mailServerUsername: string;
        mailServerUseSSL: boolean;
        defaultReplyToAddress: string;
        useTenantCodeInUrl: boolean;
        showTenantCodeFieldOnLoginForm: boolean;
        showTenantListingWhenMissingTenantCode: boolean;
    }

    interface authenticate {
        username: string;
        password: string;
        tenantCode: string;
    }

    interface booleanResponse {
        messages: string[];
        result: boolean;
    }

    interface department extends actionResponseObject {
        departmentId: string;
        tenantId: string;
        departmentName: string;
        activeDirectoryNames: string;
        enabled: boolean;
        departmentGroupId: string;
    }

    interface departmentGroup extends actionResponseObject {
        departmentGroupId: string;
        tenantId: string;
        departmentGroupName: string;
    }

    interface dictionary {
        key: string;
        value: any;
    }

    interface externalDataSource {
        name: string;
        type: string;
        connectionString: string;
        source: string;
        sortOrder: number;
        active: boolean;
    }

    interface fileStorage extends actionResponseObject {
        fileId: string;
        tenantId: string;
        itemId: string;
        fileName: string;
        extension: string;
        sourceFileId: string;
        bytes: number;
        value: any[];
        uploadDate: string;
        userId: string;
        base64value: string;
    }

    interface filter extends actionResponseObject {
        tenantId: string;
        executionTime: number;
        loading: boolean;
        showFilters: boolean;
        start: Date;
        end: Date;
        keyword: string;
        sort: string;
        sortOrder: string;
        recordsPerPage: number;
        pageCount: number;
        recordCount: number;
        page: number;
        tenants: string[];
        columns: filterColumn[];
        records: any[];
        cultureCode: string;
    }

    interface filterColumn {
        align: string;
        label: string;
        tipText: string;
        dataElementName: string;
        dataType: string;
        sortable: boolean;
        class: string;
        booleanIcon: string;
    }

    interface filterUsers extends filter {
        filterDepartments: string[];
        enabled: string;
        admin: string;
        udf01: string;
        udf02: string;
        udf03: string;
        udf04: string;
        udf05: string;
        udf06: string;
        udf07: string;
        udf08: string;
        udf09: string;
        udf10: string;
    }

    interface language {
        culture: string;
        phrases: optionPair[];
    }

    interface listItem extends actionResponseObject {
        id: string;
        tenantId: string;
        type: string;
        name: string;
        sortOrder: number;
        enabled: boolean;
    }

    interface optionPair {
        id: string;
        value: string;
    }

    interface signalRUpdate {
        tenantId: string;
        requestId: string;
        itemId: string;
        userId: string;
        updateTypeString: string;
        message: string;
        object: any;
    }

    interface setting extends actionResponseObject {
        settingId: number;
        settingName: string;
        settingType: string;
        settingNotes: string;
        settingText: string;
        tenantId: string;
        userId: string;
    }

    interface simplePost {
        singleItem: string;
        items: string[];
    }

    interface simpleResponse {
        result: boolean;
        message: string;
    }

    interface tenant extends actionResponseObject {
        tenantId: string;
        name: string;
        tenantCode: string;
        enabled: boolean;
        departments: department[];
        departmentGroups: departmentGroup[];
        tenantSettings: tenantSettings;
        listItems: listItem[];
        udfLabels: udfLabel[];
    }

    interface tenantList {
        tenantId: string;
        name: string;
        tenantCode: string;
    }

    interface tenantSettings {
        allowUsersToManageAvatars: boolean;
        allowUsersToManageBasicProfileInfo: boolean;
        allowUsersToManageBasicProfileInfoElements: string[];
        allowUsersToResetPasswordsForLocalLogin: boolean;
        allowUsersToSignUpForLocalLogin: boolean;
        cookieDomain: string;
        customAuthenticationCode: string;
        customAuthenticationName: string;
        defaultCultureCode: string;
        defaultReplyToAddress: string;
        eitSsoUrl: string;
        jasonWebTokenKey: string;
        ldapLookupRoot: string;
        ldapLookupUsername: string;
        ldapLookupPassword: string;
        ldapLookupSearchBase: string;
        ldapLookupLocationAttribute: string;
        ldapLookupPort: number;
        loginOptions: string[];
        moduleHideElements: string[];
        workSchedule: workSchedule;
        requirePreExistingAccountToLogIn: boolean;
        externalUserDataSources: externalDataSource[];
    }

    interface udfLabel extends actionResponseObject {
        id: string;
        tenantId: string;
        module: string;
        udf: string;
        label: string;
        showColumn: boolean;
        showInFilter: boolean;
        includeInSearch: boolean;
        filterOptions: string[];
    }

    interface user extends actionResponseObject {
        userId: string;
        tenantId: string;
        firstName: string;
        lastName: string;
        displayName: string;
        email: string;
        phone: string;
        username: string;
        employeeId: string;
        departmentId: string;
        departmentName: string;
        title: string;
        location: string;
        enabled: boolean;
        lastLogin: string;
        admin: boolean;
        appAdmin: boolean;
        photo: string;
        password: string;
        preventPasswordChange: boolean;
        hasLocalPassword: boolean;
        authToken: string;
        lastLockoutDate: Date;
        tenants: tenant[];
        userTenants: userTenant[];
        source: string;
        udf01: string;
        udf02: string;
        udf03: string;
        udf04: string;
        udf05: string;
        udf06: string;
        udf07: string;
        udf08: string;
        udf09: string;
        udf10: string;
    }

    interface userGroup extends actionResponseObject {
        groupId: string;
        tenantId: string;
        name: string;
        enabled: boolean;
        users: string[];
        settings: userGroupSettings;
    }

    interface userGroupSettings {
        someSetting: string;
    }

    interface userPasswordReset extends actionResponseObject {
        userId: string;
        tenantId: string;
        currentPassword: string;
        newPassword: string;
    }

    interface userTenant {
        userId: string;
        tenantId: string;
        tenantCode: string;
        tenantName: string;
    }

    interface versionInfo {
        released: Date;
        runningSince: number;
        version: string;
    }

   interface workSchedule {
        sunday: boolean;
        sundayAllDay: boolean;
        sundayStart: string;
        sundayEnd: string;

        monday: boolean;
        mondayAllDay: boolean;
        mondayStart: string;
        mondayEnd: string;

        tuesday: boolean;
        tuesdayAllDay: boolean;
        tuesdayStart: string;
        tuesdayEnd: string;

        wednesday: boolean;
        wednesdayAllDay: boolean;
        wednesdayStart: string;
        wednesdayEnd: string;

        thursday: boolean;
        thursdayAllDay: boolean;
        thursdayStart: string;
        thursdayEnd: string;

        friday: boolean;
        fridayAllDay: boolean;
        fridayStart: string;
        fridayEnd: string;

        saturday: boolean;
        saturdayAllDay: boolean;
        saturdayStart: string;
        saturdayEnd: string;
    }
}