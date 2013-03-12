﻿#region License
// <copyright file="HelpVerbOptionAttribute.cs" company="Giacomo Stelluti Scala">
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
using System;
using System.Reflection;
using CommandLine.Extensions;

using CommandLine.Infrastructure;
#endregion

namespace CommandLine
{
    /// <summary>
    /// Indicates the instance method that must be invoked when it becomes necessary show your help screen.
    /// The method signature is an instance method with that accepts and returns a <see cref="System.String"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class HelpVerbAttribute : VerbAttribute
    {
        private const string DefaultHelpText = "Display more information on a specific command.";

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpVerbAttribute"/> class.
        /// Although it is possible, it is strongly discouraged redefine the long name for this option
        /// not to disorient your users.
        /// </summary>
        public HelpVerbAttribute()
            : this("help")
        {
            HelpText = DefaultHelpText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpVerbAttribute"/> class
        /// with the specified long name. Use parameter less constructor instead.
        /// </summary>
        /// <param name="name">Help verb option alternative name.</param>
        /// <remarks>
        /// It's highly not recommended change the way users invoke help. It may create confusion.
        /// </remarks>
        public HelpVerbAttribute(string name)
            : base(name)
        {
            HelpText = DefaultHelpText;

        }
    }
}