﻿using System;
using System.Collections.Generic;
using JDI.Light.Interfaces;
using JDI.Light.Matchers;
using JDI.Light.Settings;

namespace JDI.Light.Tests.Asserts
{
    public class NUnitAsserter : BaseAsserter
    {
        public override void ThrowFail(string message)
        {
            JDISettings.Logger.Error(message);
            NUnit.Framework.Assert.Fail(message);
        }
    }
}