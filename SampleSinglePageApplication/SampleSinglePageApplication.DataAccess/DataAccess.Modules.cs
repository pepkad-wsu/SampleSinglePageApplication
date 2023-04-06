namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public DataObjects.BooleanResponse AddModule(DataObjects.AddModule module)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        bool debugMode = false;
#if DEBUG
        debugMode = true;
#endif
        if (!debugMode) {
            output.Messages.Add("This feature is only available in debug mode.");
            return (output);
        }

        if (String.IsNullOrWhiteSpace(module.Module)) {
            output.Messages.Add("Missing Required Module");
        }
        if (String.IsNullOrWhiteSpace(module.Name)) {
            output.Messages.Add("Missing Required Module Name");
        } else if (module.Name.Contains(" ")) {
            output.Messages.Add("Spaces in Module Name Not Allowed");
        }

        if (output.Messages.Any()) {
            return (output);
        }

        string moduleName = StringValue(module.Name);
        string moduleNameCamelCase = Utilities.CamelCase(module.Name);
        string moduleNameLowerCase = moduleName.ToLower();

        var dir = System.IO.Directory.GetCurrentDirectory();
        if (!String.IsNullOrWhiteSpace(dir)) {
            string layoutFile = System.IO.Path.Combine(dir, "Views\\Shared\\_Layout.cshtml");
            if (System.IO.File.Exists(layoutFile)) {
                string layout = System.IO.File.ReadAllText(layoutFile);

                if (!String.IsNullOrWhiteSpace(layout)) {
                    // Make sure all required tags exist.
                    string tagBeginMenuPublic = "<!-- BEGIN AUTO-GENERATED MENU PUBLIC -->";
                    string tagEndMenuPublic = "<!-- END AUTO-GENERATED MENU PUBLIC -->";
                    int beginMenuPublic = layout.IndexOf(tagBeginMenuPublic);
                    int endMenuPublic = layout.IndexOf(tagEndMenuPublic);

                    string tagBeginMenuLoggedIn = "<!-- AUTO-GENERATED MENU LOGGED IN -->";
                    string tagEndMenuLoggedIn = "<!-- END AUTO-GENERATED MENU LOGGED IN -->";
                    int beginMenuLoggedIn = layout.IndexOf(tagBeginMenuLoggedIn);
                    int endMenuLoggedIn = layout.IndexOf(tagEndMenuLoggedIn);

                    string tagBeginMenuAdmin = "<!-- AUTO-GENERATED MENU ADMIN-->";
                    string tagEndMenuAdmin = "<!-- END AUTO-GENERATED MENU ADMIN-->";
                    int beginMenuAdmin = layout.IndexOf(tagBeginMenuAdmin);
                    int endMenuAdmin = layout.IndexOf(tagEndMenuAdmin);

                    string tagBeginMenuAppAdmin = "<!-- AUTO-GENERATED MENU APPADMIN-->";
                    string tagEndMenuAppAdmin = "<!-- END AUTO-GENERATED MENU APPADMIN-->";
                    int beginMenuAppAdmin = layout.IndexOf(tagBeginMenuAppAdmin);
                    int endMenuAppAdmin = layout.IndexOf(tagEndMenuAppAdmin);

                    string tagBeginPartialsPublic = "<!-- BEGIN PUBLIC PARTIALS -->";
                    string tagEndPartialsPublic = "<!-- END PUBLIC PARTIALS -->";
                    int beginPartialsPublic = layout.IndexOf(tagBeginPartialsPublic);
                    int endPartialsPublic = layout.IndexOf(tagEndPartialsPublic);

                    string tagBeginPartialsLoggedIn = "<!-- BEGIN LOGGED IN PARTIALS -->";
                    string tagEndPartialsLoggedIn = "<!-- END LOGGED IN PARTIALS -->";
                    int beginPartialsLoggedIn = layout.IndexOf(tagBeginPartialsLoggedIn);
                    int endPartialsLoggedIn = layout.IndexOf(tagEndPartialsLoggedIn);

                    string tagBeginPartialsAdmin = "<!-- BEGIN ADMIN PARTIALS -->";
                    string tagEndPartialsAdmin = "<!-- END ADMIN PARTIALS -->";
                    int beginPartialsAdmin = layout.IndexOf(tagBeginPartialsAdmin);
                    int endPartialsAdmin = layout.IndexOf(tagEndPartialsAdmin);

                    string tagBeginPartialsAppAdmin = "<!-- BEGIN APPADMIN PARTIALS -->";
                    string tagEndPartialsAppAdmin = "<!-- END APPADMIN PARTIALS -->";
                    int beginPartialsAppAdmin = layout.IndexOf(tagBeginPartialsAppAdmin);
                    int endPartialsAppAdmin = layout.IndexOf(tagEndPartialsAppAdmin);

                    string tagBeginModelsPublic = "// <BEGIN PUBLIC MODELS>";
                    string tagEndModelsPublic = "// <END PUBLIC MODELS>";
                    int beginModelsPublic = layout.IndexOf(tagBeginModelsPublic);
                    int endModelsPublic = layout.IndexOf(tagEndModelsPublic);

                    string tagBeginModelsLoggedIn = "// <BEGIN LOGGED IN MODELS>";
                    string tagEndModelsLoggedIn = "// <END LOGGED IN MODELS>";
                    int beginModelsLoggedIn = layout.IndexOf(tagBeginModelsLoggedIn);
                    int endModelsLoggedIn = layout.IndexOf(tagEndModelsLoggedIn);

                    string tagBeginModelsAdmin = "// <BEGIN ADMIN MODELS>";
                    string tagEndModelsAdmin = "// <END ADMIN MODELS>";
                    int beginModelsAdmin = layout.IndexOf(tagBeginModelsAdmin);
                    int endModelsAdmin = layout.IndexOf(tagEndModelsAdmin);

                    string tagBeginModelsAppAdmin = "// <BEGIN APPADMIN MODELS>";
                    string tagEndModelsAppAdmin = "// <END APPADMIN MODELS>";
                    int beginModelsAppAdmin = layout.IndexOf(tagBeginModelsAppAdmin);
                    int endModelsAppAdmin = layout.IndexOf(tagEndModelsAppAdmin);

                    int menuStart = -1;
                    int menuEnd = -1;
                    string menuStartTag = String.Empty;
                    string menuEndTag = String.Empty;

                    int partialsStart = -1;
                    int partialsEnd = -1;
                    string partialsStartTag = String.Empty;
                    string partialsEndTag = String.Empty;

                    int modelsStart = -1;
                    int modelsEnd = -1;
                    string modelsStartTag = String.Empty;
                    string modelsEndTag = String.Empty;

                    bool menuAdmin = false;
                    bool menuAppAdmin = false;

                    switch (StringValue(module.Module).ToLower()) {
                        case "public":
                            menuStart = beginMenuPublic;
                            menuEnd = endMenuPublic;
                            menuStartTag = tagBeginMenuPublic;
                            menuEndTag = tagEndMenuPublic;

                            partialsStart = beginPartialsPublic;
                            partialsEnd = endPartialsPublic;
                            partialsStartTag = tagBeginPartialsPublic;
                            partialsEndTag = tagEndPartialsPublic;

                            modelsStart = beginModelsPublic;
                            modelsEnd = endModelsPublic;
                            modelsStartTag = tagBeginModelsPublic;
                            modelsEndTag = tagEndModelsPublic;
                            break;

                        case "loggedin":
                            menuStart = beginMenuLoggedIn;
                            menuEnd = endMenuLoggedIn;
                            menuStartTag = tagBeginMenuLoggedIn;
                            menuEndTag = tagEndMenuLoggedIn;

                            partialsStart = beginPartialsLoggedIn;
                            partialsEnd = endPartialsLoggedIn;
                            partialsStartTag = tagBeginPartialsLoggedIn;
                            partialsEndTag = tagEndPartialsLoggedIn;

                            modelsStart = beginModelsLoggedIn;
                            modelsEnd = endModelsLoggedIn;
                            modelsStartTag = tagBeginModelsLoggedIn;
                            modelsEndTag = tagEndModelsLoggedIn;
                            break;

                        case "admin":
                            menuAdmin = true;
                            menuStart = beginMenuAdmin;
                            menuEnd = endMenuAdmin;
                            menuStartTag = tagBeginMenuAdmin;
                            menuEndTag = tagEndMenuAdmin;

                            partialsStart = beginPartialsAdmin;
                            partialsEnd = endPartialsAdmin;
                            partialsStartTag = tagBeginPartialsAdmin;
                            partialsEndTag = tagEndPartialsAdmin;

                            modelsStart = beginModelsAdmin;
                            modelsEnd = endModelsAdmin;
                            modelsStartTag = tagBeginModelsAdmin;
                            modelsEndTag = tagEndModelsAdmin;
                            break;

                        case "appadmin":
                            menuAppAdmin = true;
                            menuStart = beginMenuAppAdmin;
                            menuEnd = endMenuAppAdmin;
                            menuStartTag = tagBeginMenuAppAdmin;
                            menuEndTag = tagEndMenuAppAdmin;

                            partialsStart = beginPartialsAppAdmin;
                            partialsEnd = endPartialsAppAdmin;
                            partialsStartTag = tagBeginPartialsAppAdmin;
                            partialsEndTag = tagEndPartialsAppAdmin;

                            modelsStart = beginModelsAppAdmin;
                            modelsEnd = endModelsAppAdmin;
                            modelsStartTag = tagBeginModelsAppAdmin;
                            modelsEndTag = tagEndModelsAppAdmin;
                            break;
                    }

                    if (menuStart == -1) {
                        output.Messages.Add("Missing Layout Tag: " + menuStartTag);
                    }
                    if (menuEnd == -1) {
                        output.Messages.Add("Missing Layout Tag: " + menuEndTag);
                    }
                    if (partialsStart == -1) {
                        output.Messages.Add("Missing Layout Tag: " + partialsStartTag);
                    }
                    if (partialsEnd == -1) {
                        output.Messages.Add("Missing Layout Tag: " + partialsEndTag);
                    }
                    if (modelsStart == -1) {
                        output.Messages.Add("Missing Layout Tag: " + modelsStartTag);
                    }
                    if (modelsEnd == -1) {
                        output.Messages.Add("Missing Layout Tag: " + modelsEndTag);
                    }

                    if (output.Messages.Any()) {
                        return (output);
                    } else {
                        string currentPartials = Utilities.GetTextBetweenItems(layout, partialsStartTag, partialsEndTag);
                        // Make sure we haven't already added a partial with the same name
                        string newPartialName = "_partial" + moduleName;
                        if (currentPartials.Contains(newPartialName)) {
                            output.Messages.Add("The module name '" + moduleName + "' already appears to be in use.");
                            return (output);
                        }

                        // Attempt to create the new _partial file.
                        string newPartialPath = System.IO.Path.Combine(dir, "Views\\Shared\\" + newPartialName + ".cshtml");
                        if (System.IO.File.Exists(newPartialPath)) {
                            output.Messages.Add("The new partial file '" + newPartialPath + "' already exists.");
                            return (output);
                        }

                        string newPartialContent =
                        """
						        <script src="~/js/viewModels/{ITEMNAMELOWER}.js?v=@Html.Raw(Guid.NewGuid().ToString().Replace("-", ""))"></script>
						        <div id="view-{ITEMNAMELOWER}">
						            <div data-bind="visible:MainModel().CurrentView() == '{ITEMNAMELOWER}'">
						                <div data-bind="css:{fixed: MainModel().StickyMenus() == true}">
						                    <i class="sticky-menu-icon" data-bind="html:MainModel().StickyIcon(), click:function(){MainModel().ToggleStickyMenus()}"></i>
						                    <h1 class="display-7"><i data-bind="html:MainModel().IconAndText('{ITEMNAME}')"></i></h1>
						                </div>

						                <div class="mb-2">
						                    Create your model content here for {ITEMNAME}
						                </div>
						            </div>
						        </div>
						        """;

                        newPartialContent = newPartialContent.Replace("{ITEMNAMELOWER}", moduleNameLowerCase).
                            Replace("{ITEMNAME}", moduleName);

                        try {
                            System.IO.File.WriteAllText(newPartialPath, newPartialContent);
                        } catch (Exception ex) {
                            output.Messages.Add("Error creating new partial view file '" + newPartialPath + "' - " + ex.Message);
                            return (output);
                        }

                        if (!System.IO.File.Exists(newPartialPath)) {
                            output.Messages.Add("Unable to create new partial view file '" + newPartialPath + "'");
                            return (output);
                        }

                        // Create the new model TypeScript file
                        string newTypeScriptFileName = moduleNameCamelCase + ".ts";
                        string newTypeScriptFilePath = System.IO.Path.Combine(dir, "wwwroot\\js\\viewModels\\" + newTypeScriptFileName);
                        if (System.IO.File.Exists(newTypeScriptFilePath)) {
                            output.Messages.Add("The new TypeScript file '" + newTypeScriptFilePath + "' already exists.");
                            return (output);
                        }

                        string newTypeScriptContent =
                        """
                                class {ITEMNAME}Model {
                                    MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);

                                    constructor() {
                                        this.MainModel().View.subscribe(() => {
                                            this.ViewChanged();
                                        });
                                    }

                                    /**
                                    * Function placeholder for your Add method
                                    */
                                    Add(): void {

                                    }

                                    /**
                                    * Function placeholder for your Edit method
                                    */
                                    Edit(): void {
                                
                                    }
                                
                                    /**
                                    * Function placeholder for your Load method that gets called when this view is loaded
                                    */
                                    Load(): void {

                                    }

                                    /**
                                    * Function placeholder for your Save method
                                    */
                                    Save(): void {
                                
                                    }
                                
                                    /**
                                    * Called when the view changes in the MainModel to do any necessary work in this viewModel.
                                    */
                                    ViewChanged() {
                                        switch (this.MainModel().CurrentView()) {
                                            case "{ITEMNAMELOWER}":
                                                this.Load();
                                                break;
                                        }
                                    }
                                }
                                """;

                        newTypeScriptContent = newTypeScriptContent.Replace("{ITEMNAME}", moduleName)
                            .Replace("{ITEMNAMELOWER}", moduleNameLowerCase);

                        try {
                            System.IO.File.WriteAllText(newTypeScriptFilePath, newTypeScriptContent);
                        } catch (Exception ex) {
                            output.Messages.Add("Error creating new TypeScript file '" + newTypeScriptFilePath + "' - " + ex.Message);
                            return (output);
                        }

                        if (!System.IO.File.Exists(newTypeScriptFilePath)) {
                            output.Messages.Add("Unable to create new TypeScript file '" + newTypeScriptFilePath + "'");
                            return (output);
                        }


                        // Add the new partial item.
                        List<string> newPartial = new List<string> { "@await Html.PartialAsync(\"_partial" + moduleName + "\")" };
                        layout = Utilities.AddContentToSection(layout, partialsStartTag, partialsEndTag, newPartial);

                        string currentMenu = Utilities.GetTextBetweenItems(layout, menuStartTag, menuEndTag);
                        string currentModels = Utilities.GetTextBetweenItems(layout, modelsStartTag, modelsEndTag);

                        string newMenuItem = "<li class=\"nav-item\"><a href=\"#\" class=\"nav-link\" data-bind=\"click:function(){ Nav('{ITEMNAME}'); }, css:{ active: CurrentView() == '{ITEMNAMELOWER}'}, html:IconAndText('{ITEMNAME}')\"></a></li>";
                        if (menuAdmin || menuAppAdmin) {
                            newMenuItem = "<li><a class=\"dropdown-item app-admin-only\" href=\"#\" data-bind=\"click:function(){ Nav('{ITEMNAME}'); }, css: { active: CurrentView() == '{ITEMNAMELOWER}'}, html:IconAndText('{ITEMNAME}')\"></a></li>";
                            if (!menuAppAdmin) {
                                newMenuItem = newMenuItem.Replace(" app-admin-only", "");
                            }
                        }
                        newMenuItem = newMenuItem.Replace("{ITEMNAMELOWER}", moduleNameLowerCase).
                            Replace("{ITEMNAME}", moduleName);

                        // Add the new menu item.
                        layout = Utilities.AddContentToSection(layout, menuStartTag, menuEndTag, new List<string> { newMenuItem });

                        // Add the new models item.
                        List<string> newModels = new List<string>();
                        newModels.Add("window." + moduleNameCamelCase + "Model = new " + moduleName + "Model();");
                        newModels.Add("ko.applyBindings(window." + moduleNameCamelCase + "Model, document.getElementById('view-" + moduleNameLowerCase + "'));");
                        newModels.Add("window.models.push(\"" + moduleName + "Model\");");
                        layout = Utilities.AddContentToSection(layout, modelsStartTag, modelsEndTag, newModels);

                        try {
                            // Update the layout page
                            System.IO.File.WriteAllText(layoutFile, layout);
                            output.Result = true;

                            // Don't return any errors for this, but if this is for the Admin or AppAdmin areas
                            // try and add this to the CurrentViewAdmin function in the main.ts TypeScript file.
                            string mainTypeScriptPath = System.IO.Path.Combine(dir, "wwwroot\\js\\viewModels\\main.ts");
                            if (System.IO.File.Exists(mainTypeScriptPath)) {
                                string mainTypeScript = System.IO.File.ReadAllText(mainTypeScriptPath);
                                if (!String.IsNullOrWhiteSpace(mainTypeScript)) {
                                    bool modifiedMain = false;

                                    if (menuAdmin || menuAppAdmin) {
                                        string tsBeginAdminMenuItems = "// <BEGIN ADMIN MENU ITEMS>";
                                        string tsEndAdminMenuItems = "// <END ADMIN MENU ITEMS>";
                                        if (mainTypeScript.Contains(tsBeginAdminMenuItems) && mainTypeScript.Contains(tsEndAdminMenuItems)) {
                                            mainTypeScript = Utilities.AddContentToSection(mainTypeScript, tsBeginAdminMenuItems, tsEndAdminMenuItems,
                                                new List<string> { "case \"" + moduleNameLowerCase + "\":" });
                                            modifiedMain = true;
                                        }
                                    }

                                    string tsBeginIconItems = "// <BEGIN ICON ITEMS>";
                                    string tsEndIconItems = "// <END ICON ITEMS>";
                                    if (mainTypeScript.Contains(tsBeginIconItems) && mainTypeScript.Contains(tsEndIconItems)) {
                                        List<string> newIcon = new List<string>();
                                        newIcon.Add("case \"" + moduleNameLowerCase + "\":");
                                        newIcon.Add("  output = '<i class=\"fa-solid fa-circle-question\"></i>';");
                                        newIcon.Add("  break;");

                                        mainTypeScript = Utilities.AddContentToSection(mainTypeScript, tsBeginIconItems, tsEndIconItems, newIcon);
                                        modifiedMain = true;
                                    }

                                    if (modifiedMain) {
                                        System.IO.File.WriteAllText(mainTypeScriptPath, mainTypeScript);
                                    }
                                }
                            }

                            // Call hot reload?
                            // For the time being force the app to stop debugging so it can be restarted.
                            // In the future if I find a way to force hot reload from code I will do that.
                            // _hostLifetime.StopApplication();
                        } catch (Exception ex) {
                            output.Messages.Add("Error updating _Layout - " + ex.Message);
                        }
                    }
                } else {
                    output.Messages.Add("Unable to read the contents of the Views\\Shared\\_Layout.cshtml file.");
                }
            } else {
                output.Messages.Add("Unable to find the Views\\Shared\\_Layout.cshtml File");
            }
        } else {
            output.Messages.Add("Unable to get current directory.");
        }

        return output;
    }
}