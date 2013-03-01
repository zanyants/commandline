﻿#region License
// <copyright file="TargetCapabilitiesExtensions.cs" company="Giacomo Stelluti Scala">
//   Copyright 2015-2013 Giacomo Stelluti Scala
// </copyright>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion
#region Using Directives

using System.Linq;

using CommandLine.Infrastructure;
#endregion

namespace CommandLine
{
    internal static class CapabilitiesExtensions
    {
        public static bool AnyVerbs<T>(this T options)
        {
            return Metadata.GetAttributes(options).Any(a => a.Item2 is VerbOptionAttribute);
        }

        public static bool HasHelp<T>(this T options)
        {
            return Metadata.GetAttributes(options).Count(a => a.Item2 is HelpOptionAttribute) == 1;
        }

        public static bool HasVerbHelp<T>(this T options)
        {
            return Metadata.GetAttributes(options).Count(a => a.Item2 is HelpVerbOptionAttribute) == 1;
        }

        public static bool CanReceiveParserState<T>(this T options)
        {
            return Metadata.GetAttributes(options).Count(a => a.Item2 is ParserStateAttribute) == 1;
        }
    }
}