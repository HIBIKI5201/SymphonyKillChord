using System;
using System.Collections.Generic;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class EnvironmentValidator
    {


        public static bool Validate(params EnvironmentVariable[] variables)
        {
            bool hasError = false;
            foreach (EnvironmentVariable variable in variables)
            {
                if (string.IsNullOrEmpty(variable.Value))
                {
                    Console.WriteLine($"環境変数 {variable.Key} が見つかりませんでした。");
                    hasError = true;
                }
            }
            return hasError;
        }
    }
}
