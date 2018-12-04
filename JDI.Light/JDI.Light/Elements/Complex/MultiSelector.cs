﻿using System;
using System.Collections.Generic;
using System.Linq;
using JDI.Light.Elements.Composite;
using JDI.Light.Extensions;
using JDI.Light.Interfaces.Base;
using JDI.Light.Utils;
using OpenQA.Selenium;

namespace JDI.Light.Elements.Complex
{
    public abstract class MultiSelector<TEnum> : BaseSelector<TEnum>, IMultiSelector<TEnum>
        where TEnum : IConvertible
    {
        private string _separator = ", ";

        public Action<MultiSelector<TEnum>> ClearAction = m =>
        {
            if (!m.HasLocator && m.AllLabels == null)
                throw JDI.Assert.Exception("Can't clear options. No optionsNamesLocator and allLabelsLocator found");
            if (m.Locator.ToString().Contains("{0}"))
                throw JDI.Assert.Exception(
                    "Can't clear options. Specify allLabelsLocator or fix optionsNamesLocator (should not contain '{0}')");
            if (m.AllLabels != null)
            {
                m.ClearElements(m.AllLabels.WebElements);
                return;
            }

            var elements = m.WebElements;
            if (elements.Count == 1 && elements[0].TagName.Equals("select"))
                if (m.Selector.Options.Any())
                {
                    m.Selector.DeselectAll();
                    return;
                }
                else
                {
                    throw JDI.Assert.Exception(
                        $"<select> tag has no <option> tags. Please Clarify element locator ({m})");
                }

            if (elements.Count == 1 && elements[0].TagName.Equals("ul"))
                elements = elements[0].FindElements(By.TagName("li")).ToList();
            m.ClearElements(elements);
        };

        protected Action<MultiSelector<TEnum>, IList<int>> SelectListIndexesAction =
            (m, nums) => nums.ForEach(num => m.SelectNumAction(m, num));

        protected Action<MultiSelector<TEnum>, IList<string>> SelectListNamesAction =
            (m, names) => names.ForEach(name => m.SelectNameAction(m, name));

        protected MultiSelector(By optionsNamesLocator = null, By allLabelsLocator = null)
            : base(optionsNamesLocator, allLabelsLocator)
        {
            SelectedNameAction = (m, name) => SelectedElementAction(this, GetWebElement(name));
            SelectedNumAction = (m, num) => SelectedElementAction(this, GetWebElement(num));
            GetValueAction = m => AreSelected().FormattedJoin();
            GetWebElementFunc = (s, name) =>
            {
                var ms = (MultiSelector<TEnum>) s;
                if (!ms.HasLocator && ms.AllLabels == null)
                    throw JDI.Assert.Exception("Can't get option. No optionsNamesLocator and allLabelsLocator found");
                if (ms.Locator.ToString().Contains("{0}"))
                    return new CompositeUIElement(ms.Locator.FillByTemplate(name))
                        .WebElements[0];
                if (ms.AllLabels != null)
                    return ms.GetWebElement(AllLabels.WebElements, name);
                return ms.GetWebElement(GetElementsFromTag(), name);
            };
        }

        public override Action<BaseSelector<TEnum>, string> SetValueAction =>
            (c, value) => SelectListNamesAction(this, value.Split(_separator));

        public void Select(params string[] names)
        {
            Invoker.DoAction($"Select '{names.FormattedJoin()}'", () => SelectListNamesAction(this, names));
        }

        public void Select(params TEnum[] names)
        {
            Select(names.Select(name => name.ToString()).ToArray());
        }

        public void Select(params int[] nums)
        {
            Invoker.DoAction($"Select '{nums.FormattedJoin()}'", () => SelectListIndexesAction(this, nums));
        }

        public void Check(params string[] names)
        {
            Clear();
            Select(names);
        }

        public void Check(params TEnum[] names)
        {
            Clear();
            Select(names);
        }

        public void Check(params int[] nums)
        {
            Clear();
            Select(nums);
        }

        public void Uncheck(params string[] names)
        {
            CheckAll();
            Select(names);
        }

        public void Uncheck(params TEnum[] names)
        {
            CheckAll();
            Select(names);
        }

        public void Uncheck(params int[] nums)
        {
            CheckAll();
            Select(nums);
        }

        public IList<string> AreSelected()
        {
            return Invoker.DoActionWithResult("Are selected", () =>
                Names.Where(name => SelectedNameAction(this, name)).ToList());
        }

        public void WaitSelected(params TEnum[] names)
        {
            WaitSelected(names.Select(name => name.ToString()).ToArray());
        }

        public void WaitSelected(params string[] names)
        {
            var result = Invoker.DoActionWithResult($"Are selected '{names.FormattedJoin()}'",
                () => names.All(name => Timer.Wait(() => SelectedNameAction(this, name))));
            JDI.Assert.IsTrue(result);
        }

        public IList<string> AreDeselected()
        {
            return Invoker.DoActionWithResult("Are deselected", () =>
               Names.Where(name => !Timer.Wait(() => SelectedNameAction(this, name))).ToList());
        }

        public void WaitDeselected(params TEnum[] names)
        {
            WaitDeselected(names.Select(name => name.ToString()).ToArray());
        }

        public void WaitDeselected(params string[] names)
        {
            var result = Invoker.DoActionWithResult($"Wait deselected '{names.FormattedJoin()}'",
                () => names.All(name => !Timer.Wait(() => SelectedNameAction(this, name))));
            JDI.Assert.IsTrue(result);
        }

        public void Clear()
        {
            Invoker.DoAction("Clear Options", () => ClearAction(this));
        }

        public void CheckAll()
        {
            Options.Where(label => !SelectedNameAction(this, label)).ForEach(label => SelectNameAction(this, label));
        }

        public void SelectAll()
        {
            CheckAll();
        }

        public void UncheckAll()
        {
            Clear();
        }

        public string GetValue()
        {
            return Value;
        }

        private void ClearElements(IList<IWebElement> els)
        {
            els.Where(el => SelectedNameAction(this, el.Text)).ForEach(el => el.Click());
        }

        private IWebElement GetWebElement(IList<IWebElement> els, string name)
        {
            if (els == null)
                throw JDI.Assert.Exception("Can't get option. No optionsNamesLocator and allLabelsLocator found");
            var elements = els.Where(el => el.Text.Equals(name)).ToList();
            if (elements.Count == 1)
                return elements[0];
            throw JDI.Assert.Exception("Can't get option. No optionsNamesLocator and allLabelsLocator found");
        }

        protected IWebElement GetWebElement(int num)
        {
            if (!HasLocator && AllLabels == null)
                throw JDI.Assert.Exception("Can't get option. No optionsNamesLocator and allLabelsLocator found");
            if (Locator.ToString().Contains("{0}"))
                throw JDI.Assert.Exception(
                    "Can't get options. Specify allLabelsLocator or fix optionsNamesLocator (should not contain '{0}')");
            if (AllLabels != null)
                return GetWebElement(AllLabels.WebElements, num);
            return GetWebElement(GetElementsFromTag(), num);
        }

        private IWebElement GetWebElement(IList<IWebElement> els, int num)
        {
            if (num <= 0)
                throw JDI.Assert.Exception($"Can't get option with num '{num}'. Number should be 1 or more");
            if (num > els.Count)
                throw JDI.Assert.Exception($"Can't get option with num '{num}'. Found only {els.Count} options");
            return els[num - 1];
        }

        public IMultiSelector<TEnum> SetValuesSeparator(string separator)
        {
            _separator = separator;
            return this;
        }
    }
}