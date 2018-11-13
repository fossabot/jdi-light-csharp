﻿using System;
using System.Collections.Generic;
using System.Reflection;
using JDI.Core;
using JDI.Core.Attributes;
using JDI.Core.Attributes.Functions;
using JDI.Core.Interfaces.Base;
using JDI.Core.Logging;
using JDI.Core.Settings;
using JDI.Core.Utils;
using JDI.Web.Selenium.Attributes;
using JDI.Web.Selenium.DriverFactory;
using JDI.Web.Selenium.Elements.APIInteract;
using JDI.Web.Selenium.Elements.Base;
using JDI.Web.Selenium.Elements.WebActions;
using OpenQA.Selenium;

namespace JDI.Web.Selenium.Base
{
    public class WebBaseElement : IBaseElement
    {
        public static Action<string, Action<string>> DoActionRule = (text, action) =>
        {
            if (text == null) return;
            action.Invoke(text);
        };

        public static Action<string, Action<string>> SetValueEmptyAction = (text, action) =>
        {
            if (string.IsNullOrEmpty(text)) return;
            action.Invoke(text.Equals("#CLEAR#") ? "" : text);
        };

        private readonly IWebElement _webElement;
        private string _typeName;
        private string _varName;
        public ElementsActions Actions;

        public Functions Function = Functions.None;
        protected GetElementClass GetElementClass;

        public ActionInvoker Invoker;

        public WebBaseElement(By byLocator = null, IWebElement webElement = null,
            List<IWebElement> webElements = null, WebBaseElement element = null)
        {
            Invoker = new ActionInvoker(this);
            GetElementClass = new GetElementClass(this);
            Actions = new ElementsActions(this);
            WebAvatar = new GetElementModule(this, byLocator) {WebElement = webElement, WebElements = webElements};
            _webElement = webElement;
            if (element != null)
            {
                WebAvatar.DriverName = element.WebAvatar.DriverName;
                Parent = element.Parent;
            }
        }

        public By Locator => WebAvatar.ByLocator;
        public By FrameLocator => WebAvatar.FrameLocator;

        public static ActionScenarios ActionScenrios
        {
            set => ActionInvoker.ActionScenarios = value;
        }

        public GetElementModule WebAvatar
        {
            get => (GetElementModule) Avatar;
            set => Avatar = value;
        }

        private string VarName => _varName ?? Name;

        protected Timer Timer => WebAvatar.Timer;

        public IWebDriver WebDriver => WebAvatar.WebDriver;

        public IWebElement WebElement
        {
            get => _webElement ?? WebAvatar.WebElement;
            set => WebAvatar.WebElement = value;
        }

        public List<IWebElement> WebElements
        {
            get => WebAvatar.WebElements;
            set => WebAvatar.WebElements = value;
        }

        public bool HasLocator => WebAvatar.HasLocator;

        public IJavaScriptExecutor JsExecutor => (IJavaScriptExecutor) WebDriver;

        public object Parent { get; set; }

        public void SetFunction(Functions function)
        {
            Function = function;
        }

        public IAvatar Avatar { get; set; }
        public string Name { get; set; }
        public string ParentTypeName => Parent?.GetType().Name ?? "";

        public string TypeName
        {
            get => _typeName ?? GetType().Name;
            set => _typeName = value;
        }

        public void SetName(FieldInfo field)
        {
            Name = NameAttribute.GetElementName(field);
            _varName = field.Name;
        }

        public WebElement GetHighLightElement()
        {
            return Avatar.GetFirstValue<WebElement>();
        }

        public void FillLocatorTemplate(string name)
        {
            WebAvatar.ByLocator = Locator.FillByTemplate(name);
        }

        public WebBaseElement SetAvatar(GetElementModule avatar = null, By byLocator = null)
        {
            WebAvatar = (avatar ?? WebAvatar).Copy(byLocator);
            return this;
        }

        public void SetWaitTimeout(long mSeconds)
        {
            JDISettings.Logger.Debug("Set wait timeout to " + mSeconds);
            WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(mSeconds);
            JDISettings.Timeouts.CurrentTimeoutSec = (int) (mSeconds / 1000);
        }

        public void RestoreWaitTimeout()
        {
            SetWaitTimeout(JDISettings.Timeouts.WaitElementSec);
        }

        public void DoAction(string actionName, Action action, LogLevels logLevels = LogLevels.Info)
        {
            LogAction(actionName, logLevels);
            action.Invoke();
        }

        public void DoActionResult<TResult>(string actionName, Func<TResult> action,
            Func<TResult, string> logResult = null, LogLevels logLevels = LogLevels.Info)
        {
            LogAction(actionName, logLevels);
            var res = action.Invoke();
            logResult?.Invoke(res);
        }

        public void LogAction(string actionName, LogLevels level)
        {
            JDISettings.ToLog(string.Format(JDISettings.ShortLogMessagesFormat
                ? "{0} for {1}"
                : "Perform action '{0}' with WebElement ({1})", actionName, ToString()), level);
        }

        public void LogAction(string actionName)
        {
            LogAction(actionName, LogLevels.Info);
        }

        public new string ToString()
        {
            return JDISettings.ShortLogMessagesFormat
                ? $"{TypeName} '{Name}' ({ParentTypeName}.{VarName}; {Avatar})"
                : $"Name: '{Name}', Type: '{TypeName}' In: '{ParentTypeName}', {Avatar}";
        }
    }
}