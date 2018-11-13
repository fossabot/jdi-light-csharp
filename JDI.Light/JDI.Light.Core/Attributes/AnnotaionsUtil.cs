﻿using System.Linq;
using System.Reflection;
using JDI.Core.Attributes.Functions;

namespace JDI.Core.Attributes
{
    public class AnnotaionsUtil
    {
        public static string GetElementName(FieldInfo field)
        {
            var name = NameAttribute.GetName(field);
            return string.IsNullOrEmpty(name)
                ? name
                : SplitCamelCase(field.Name);
        }

        public static Functions.Functions GetFunction(FieldInfo field)
        {
            if (field.GetCustomAttribute<OkButtonAttribute>(false) != null)
                return Functions.Functions.Ok;
            if (field.GetCustomAttribute<CloseButtonAttribute>(false) != null)
                return Functions.Functions.Close;
            if (field.GetCustomAttribute<CancelButtonAttribute>(false) != null)
                return Functions.Functions.Cancel;
            return Functions.Functions.None;
        }

        private static string SplitCamelCase(string camel)
        {
            var result = camel.ToUpper().First().ToString();
            for (var i = 1; i < camel.Length - 1; i++)
                result += (IsCapital(camel[i]) && !IsCapital(camel[i - 1]) ? " " : "") + camel[i];
            return result + camel[camel.Length - 1];
        }

        private static bool IsCapital(char ch)
        {
            return 'A' < ch && ch < 'Z';
        }
    }
}