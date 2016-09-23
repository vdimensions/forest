﻿/*
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Forest.Expressions
{
    internal sealed class SimpleExpressionEvaluator : IForestExpressionEvaluator
    {
        private const string RegexPattern = @"(?<exp>(?:\${)(?:<exp>|(?:[A-Za-z0-9\.\""\'\+\-\*\/\(\)\[\]\@])+)+(?:}))";
        //private const string RegexPattern2 = "(?<=\\${)([^${}]+)(?=})";
        private static readonly Regex MatchPattern = new Regex(RegexPattern, RegexOptions.Compiled|RegexOptions.ExplicitCapture|RegexOptions.CultureInvariant);

        public SimpleExpressionEvaluator ()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="context"/> or <paramref name="expression"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="expression"/> is an empty string</exception>
        public string Evaluate(IViewContext context, string expression)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (expression.Length == 0)
            {
                throw new ArgumentException("Expression cannot be an empty string", "expression");
            }
            return DoEvaluate(context, expression);
        }

        private static string DoEvaluate(IViewContext context, string expression)
        {
            var matches = MatchPattern.Matches(expression);
            if (matches.Count == 0)
            {
                return context.EvaluateExpression(expression);
            }
            var caretIndex = 0;
            IList<string> expressions = new List<string>();
            foreach (Match match in matches)
            {
                expressions.Add(expression.Substring(caretIndex, match.Index));
                var unwrappedExpression = match.Value.Substring(0, match.Value.Length - 1).Substring(2);
                var exprValue = DoEvaluate(context, unwrappedExpression);
                expressions.Add(exprValue);
                caretIndex = match.Index + match.Length;
            }
            if (caretIndex < expression.Length - 1)
            {
                expressions.Add(expression.Substring(caretIndex));
            }
            return expressions.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s)).ToString();
        }
    }
}

