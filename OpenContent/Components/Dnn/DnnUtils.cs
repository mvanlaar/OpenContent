﻿using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Satrabel.OpenContent.Components.AppDefinitions;


namespace Satrabel.OpenContent.Components
{
    public static class DnnUtils
    {
        /// <summary>
        /// Gets the list of the DNN modules by friendlyName.
        /// </summary>
        /// <param name="friendlyName">Friendly name of the module.</param>
        /// <returns></returns>
        internal static ModuleInfo GetLastModuleByFriendlyName(string friendlyName)
        {
            //DesktopModuleController dmc = new DesktopModuleController();
            //DesktopModuleController.GetDesktopModuleByFriendlyName
            int portalid = PortalSettings.Current.PortalId;
            string culture = PortalSettings.Current.CultureCode;
            TabController tc = new TabController();
            ModuleController mc = new ModuleController();
            var modules = mc.GetModulesByDefinition(portalid, friendlyName).Cast<ModuleInfo>().OrderByDescending(m => m.ModuleID);
            foreach (var mod in modules)
            {
                var tab = tc.GetTab(mod.TabID, portalid, false);
                if (tab.CultureCode == culture || string.IsNullOrEmpty(tab.CultureCode))
                {
                    return mod;
                }
            }
            return modules.FirstOrDefault();
        }

        public static IEnumerable<OpenContentModuleInfo> GetDnnOpenContentModules(int portalId)
        {
            ModuleController mc = new ModuleController();
            ArrayList modules = mc.GetModulesByDefinition(portalId, App.Config.Opencontent);
            return modules.OfType<ModuleInfo>().Select(module => new OpenContentModuleInfo(module));
        }

        /// <summary>
        /// Gets the DNN tab by URL and culture.
        /// </summary>
        /// <param name="pageUrl">The page URL.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static TabInfo GetDnnTabByUrl(string pageUrl, string culture)
        {
            var alternativeLocale = LocaleController.Instance.GetLocale(culture);
            TabController tc = new TabController();
            var alternativeTab = tc.GetTabByCulture(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current.PortalId, alternativeLocale);
            throw new NotImplementedException();
        }

        public static int GetTabByCurrentCulture(int portalId, int tabId, string cultureCode)
        {
            var tc = new TabController();
            Locale locale = LocaleController.Instance.GetLocale(cultureCode);
            var tab = tc.GetTabByCulture(tabId, portalId, locale);
            if (tab != null)
            {
                return tab.TabID;
            }
            else
            {
                return tabId;
            }
        }

        public static bool IsPublishedTab(this TabInfo tab)
        {
            return !tab.IsDeleted &&
                   (tab.StartDate == Null.NullDate || tab.StartDate < DateTime.Now) &&
                   (tab.EndDate == Null.NullDate || tab.EndDate > DateTime.Now);
        }

        public static OpenContentSettings OpenContentSettings(this ModuleInfo module)
        {
            return new OpenContentSettings(module.ModuleSettings);
        }
        public static OpenContentSettings OpenContentSettings(this ModuleInstanceContext module)
        {
            return new OpenContentSettings(module.Settings);
        }
        public static OpenContentSettings OpenContentSettings(this PortalModuleBase module)
        {
            return new OpenContentSettings(module.Settings);
        }

        internal static void RegisterScript(Page page, string sourceFolder, string jsfilename, int jsOrder)
        {
            if (page == null) return;
            if (string.IsNullOrEmpty(jsfilename)) return;

            if (!jsfilename.StartsWith("/") && !jsfilename.Contains("//"))
            {
                jsfilename = sourceFolder + jsfilename;
            }
            else if (!jsfilename.Contains("//"))
            {
                var file = new FileUri(jsfilename);
                jsfilename = file.UrlFilePath;
            }
            ClientResourceManager.RegisterScript(page, jsfilename, jsOrder);
        }


        // for openform compatibility
        [Obsolete("This method is obsolete since dec 2015; use DnnLanguageUtils.GetCurrentCultureCode() instead")]
        public static string GetCurrentCultureCode()
        {
            return DnnLanguageUtils.GetCurrentCultureCode();
        }
        
        public static DotNetNuke.Security.SecurityAccessLevel ToDnnSecurityAccessLevel(this SecurityAccessLevel level)
        {
            switch (level)
            {
                case SecurityAccessLevel.EditRights:
                    return DotNetNuke.Security.SecurityAccessLevel.Edit;
                case SecurityAccessLevel.AdminRights:
                    return DotNetNuke.Security.SecurityAccessLevel.Admin;
                case SecurityAccessLevel.SuperUserRights:
                    return DotNetNuke.Security.SecurityAccessLevel.Host;
                default:
                    string msg = $"unknown SecurityAccessLevel {level}";
                    App.Services.Logger.Error(msg);
                    throw new NotImplementedException(msg);
            }
        }

        public static string ToDnnActionType(this ActionType itemActionType)
        {
            switch (itemActionType)
            {
                case ActionType.Add:
                    return ModuleActionType.AddContent;
                case ActionType.Edit:
                    return ModuleActionType.EditContent;
                case ActionType.Misc:
                    return ModuleActionType.ContentOptions;
                default:
                    string msg= $"unknown ActionType {itemActionType}";
                    App.Services.Logger.Error(msg);
                    throw new NotImplementedException(msg);
            }
        }
    }
}