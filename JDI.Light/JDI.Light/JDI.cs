﻿using System;
using System.Linq;
using JDI.Light.Interfaces;
using JDI.Light.Logging;
using JDI.Light.Selenium.DriverFactory;
using JDI.Light.Selenium.Elements;
using JDI.Light.Settings;
using OpenQA.Selenium;
using static JDI.Light.Utils.ExceptionUtils;

namespace JDI.Light
{
    public static class JDI
    {
        public static bool IsDemoMode;
        public static bool ShortLogMessagesFormat;
        public static bool UseCache;
        public static WebSettings WebSettings;
        public static WebCascadeInit WebInit;
        public static IDriverFactory<IWebDriver> DriverFactory;
        public static Timeouts Timeouts;
        public static IAssert Assert;
        public static ILogger Logger;

        public static IJavaScriptExecutor JsExecutor => DriverFactory.GetDriver() as IJavaScriptExecutor;

        static JDI()
        {
            Timeouts = new Timeouts();
            ShortLogMessagesFormat = true;
            WebSettings = new WebSettings();
            WebInit = new WebCascadeInit();

            GetFromPropertiesAvoidExceptions(p => DriverFactory.RegisterDriver(p), "Driver");
            GetFromPropertiesAvoidExceptions(p => DriverFactory.SetRunType(p), "RunType");
            GetFromPropertiesAvoidExceptions(p => Timeouts.WaitElementSec = int.Parse(p), "TimeoutWaitElement");
            GetFromPropertiesAvoidExceptions(p => ShortLogMessagesFormat = p.ToLower().Equals("short"), "LogMessageFormat");
            GetFromPropertiesAvoidExceptions(p =>
                UseCache = p.ToLower().Equals("true") || p.ToLower().Equals("1"), "Cache");
            GetFromPropertiesAvoidExceptions(p =>
                UseCache = p.ToLower().Equals("true") || p.ToLower().Equals("1"), "DemoMode");
            GetFromPropertiesAvoidExceptions(p => DriverFactory.DriverPath = p, "DriversFolder"); GetFromPropertiesAvoidExceptions(p =>
            {
                p = p.ToLower();
                if (p.Equals("soft"))
                    p = "any,multiple";
                if (p.Equals("strict"))
                    p = "visible,single";
                if (p.Split(',').Length != 2) return;
                var parameters = p.Split(',').ToList();
                if (parameters.Contains("visible") || parameters.Contains("displayed"))
                    DriverFactory.ElementSearchCriteria = el => el.Displayed;
                if (parameters.Contains("any") || parameters.Contains("all"))
                    DriverFactory.ElementSearchCriteria = el => el != null;
                if (parameters.Contains("single") || parameters.Contains("displayed"))
                    WebDriverFactory.OnlyOneElementAllowedInSearch = true;
                if (parameters.Contains("multiple") || parameters.Contains("displayed"))
                    WebDriverFactory.OnlyOneElementAllowedInSearch = false;
            }, "SearchElementStrategy");
        }

        public static void Init(ILogger logger = null, IAssert assert = null,
            Timeouts timeouts = null, IDriverFactory<IWebDriver> driverFactory = null)
        {
            DriverFactory = driverFactory ?? new WebDriverFactory();
            Assert = assert;
            Timeouts = timeouts ?? new Timeouts();
            Logger = logger ?? new ConsoleLogger();

            WebSettings.Init();
        }

        public static void InitSite(Type siteType)
        {
            WebInit.InitStaticPages(siteType, DriverFactory.CurrentDriverName);
        }
    }
}