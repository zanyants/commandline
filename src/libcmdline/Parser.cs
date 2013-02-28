﻿#region License
// <copyright file="Parser.cs" company="Giacomo Stelluti Scala">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using CommandLine.Parsing;
using CommandLine.Text;
#endregion

namespace CommandLine
{
    /// <summary>
    /// Provides methods to parse command line arguments.
    /// </summary>
    public sealed class Parser : IDisposable
    {
        ///// <summary>
        ///// Default exit code (1) used by <see cref="Parser.ParseArgumentsStrict(string[],object,Action)"/>
        ///// and <see cref="Parser.ParseArgumentsStrict(string[],object,Action&lt;string,object&gt;,Action)"/> overloads.
        ///// </summary>
        //public const int DefaultExitCodeFail = 1;
        private static readonly Parser DefaultParser = new Parser(true);
        private readonly ParserSettings _settings;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLine.Parser"/> class.
        /// </summary>
        public Parser()
        {
            _settings = new ParserSettings { Consumed = true };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class,
        /// configurable with a <see cref="ParserSettings"/> object.
        /// </summary>
        /// <param name="settings">The <see cref="ParserSettings"/> object is used to configure
        /// aspects and behaviors of the parser.</param>
        [Obsolete("Use constructor that accepts Action<ParserSettings>.")]
        public Parser(ParserSettings settings)
        {
            Assumes.NotNull(settings, "settings", SR.ArgumentNullException_ParserSettingsInstanceCannotBeNull);
            
            if (settings.Consumed)
            {
                throw new InvalidOperationException(SR.InvalidOperationException_ParserSettingsInstanceCanBeUsedOnce);
            }

            _settings = settings;
            _settings.Consumed = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class,
        /// configurable with <see cref="ParserSettings"/> using a delegate.
        /// </summary>
        /// <param name="configuration">The <see cref="Action&lt;ParserSettings&gt;"/> delegate used to configure
        /// aspects and behaviors of the parser.</param>
        public Parser(Action<ParserSettings> configuration)
        {
            Assumes.NotNull(configuration, "configuration", SR.ArgumentNullException_ParserSettingsDelegateCannotBeNull);

            _settings = new ParserSettings();
            configuration.Invoke(Settings);
            _settings.Consumed = true;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "singleton", Justification = "The constructor that accepts a boolean is designed to support default singleton, the parameter is ignored")]
        private Parser(bool singleton)
            : this(with =>
                {
                    with.CaseSensitive = false;
                    with.MutuallyExclusive = false;
                    with.HelpWriter = Console.Error;
                    with.ParsingCulture = CultureInfo.InvariantCulture;
                })
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CommandLine.Parser"/> class.
        /// </summary>
        ~Parser()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the singleton instance created with basic defaults.
        /// </summary>
        public static Parser Default
        {
            get { return DefaultParser; }
        }

        /// <summary>
        /// Gets the instance that implements <see cref="CommandLine.ParserSettings"/> in use.
        /// </summary>
        public ParserSettings Settings
        {
            get { return _settings; }
        }

        ///// <summary>
        ///// Parses a <see cref="System.String"/> array of command line arguments, setting values in <paramref name="options"/>
        ///// parameter instance's public fields decorated with appropriate attributes.
        ///// </summary>
        ///// <param name="args">A <see cref="System.String"/> array of command line arguments.</param>
        ///// <param name="options">An instance used to receive values.
        ///// Parsing rules are defined using <see cref="CommandLine.BaseOptionAttribute"/> derived types.</param>
        ///// <returns>True if parsing process succeed.</returns>
        ///// <exception cref="System.ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
        ///// <exception cref="System.ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        //public bool ParseArguments(string[] args, object options)
        //{
        //    Assumes.NotNull(args, "args", SR.ArgumentNullException_ArgsStringArrayCannotBeNull);
        //    Assumes.NotNull(options, "options", SR.ArgumentNullException_OptionsInstanceCannotBeNull);

        //    return DoParseArguments(args, options);
        //}

        ///// <summary>
        ///// Parses a <see cref="System.String"/> array of command line arguments with verb commands, setting values in <paramref name="options"/>
        ///// parameter instance's public fields decorated with appropriate attributes.
        ///// This overload supports verb commands.
        ///// </summary>
        ///// <param name="args">A <see cref="System.String"/> array of command line arguments.</param>
        ///// <param name="options">An instance used to receive values.
        ///// Parsing rules are defined using <see cref="CommandLine.BaseOptionAttribute"/> derived types.</param>
        ///// <param name="onVerbCommand">Delegate executed to capture verb command name and instance.</param>
        ///// <returns>True if parsing process succeed.</returns>
        ///// <exception cref="System.ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
        ///// <exception cref="System.ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        ///// <exception cref="System.ArgumentNullException">Thrown if <paramref name="onVerbCommand"/> is null.</exception>
        //public bool ParseArguments(string[] args, object options, Action<string, object> onVerbCommand)
        //{
        //    Assumes.NotNull(args, "args", SR.ArgumentNullException_ArgsStringArrayCannotBeNull);
        //    Assumes.NotNull(options, "options", SR.ArgumentNullException_OptionsInstanceCannotBeNull);
        //    Assumes.NotNull(options, "onVerbCommand", SR.ArgumentNullException_OnVerbDelegateCannotBeNull);

        //    object verbInstance = null;

        //    var result = DoParseArgumentsVerbs(args, options, ref verbInstance);
            
        //    onVerbCommand(args.FirstOrDefault() ?? string.Empty, result ? verbInstance : null);

        //    return result;
        //}

        /// <summary>
        /// Parses a <see cref="System.String"/> array of command line arguments, setting values in <paramref name="options"/>
        /// parameter instance's public fields decorated with appropriate attributes. If parsing fails, the method invokes
        /// the <paramref name="onFail"/> delegate, if null exits with <see cref="Parser.DefaultExitCodeFail"/>.
        /// </summary>
        /// <param name="args">A <see cref="System.String"/> array of command line arguments.</param>
        /// <param name="options">An object's instance used to receive values.
        /// Parsing rules are defined using <see cref="CommandLine.BaseOptionAttribute"/> derived types.</param>
        /// <param name="onFail">The <see cref="Action"/> delegate executed when parsing fails.</param>
        /// <returns>True if parsing process succeed.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public T ParseArguments<T>(string[] args, Action onFail)
            where T : new()
        {
            Assumes.NotNull(args, "args", SR.ArgumentNullException_ArgsStringArrayCannotBeNull);
            //Assumes.NotNull(options, "options", SR.ArgumentNullException_OptionsInstanceCannotBeNull);

            var optionsAndResult = DoParseArguments<T>(args);
            var result = optionsAndResult.Item1;
            var options = optionsAndResult.Item2;

            HandleDynamicAutoBuild(options);

            if (!result)
            {
                onFail();
            }

            return options;
        }

        private void HandleDynamicAutoBuild<T>(T options)
            where T : new()
        {
            if (!object.Equals(options, default(T)))
            {
                return;
            }

            if (this._settings.DynamicAutoBuild)
            {
                this.InvokeAutoBuildIfNeeded(options);
            }
        }

        /// <summary>
        /// Parses a <see cref="System.String"/> array of command line arguments with verb commands, setting values in <paramref name="options"/>
        /// parameter instance's public fields decorated with appropriate attributes. If parsing fails, the method invokes
        /// the <paramref name="onFail"/> delegate, if null exits with <see cref="Parser.DefaultExitCodeFail"/>.
        /// This overload supports verb commands.
        /// </summary>
        /// <param name="args">A <see cref="System.String"/> array of command line arguments.</param>
        /// <param name="options">An instance used to receive values.
        /// Parsing rules are defined using <see cref="CommandLine.BaseOptionAttribute"/> derived types.</param>
        /// <param name="onVerbCommand">Delegate executed to capture verb command name and instance.</param>
        /// <param name="onFail">The <see cref="Action"/> delegate executed when parsing fails.</param>
        /// <returns>True if parsing process succeed.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="onVerbCommand"/> is null.</exception>
        public T ParseArguments<T>(string[] args, Action<string, object> onVerbCommand, Action onFail)
            where T : new()
        {
            Assumes.NotNull(args, "args", SR.ArgumentNullException_ArgsStringArrayCannotBeNull);
            //Assumes.NotNull(options, "options", SR.ArgumentNullException_OptionsInstanceCannotBeNull);
            Assumes.NotNull(onVerbCommand, "onVerbCommand", SR.ArgumentNullException_OnVerbDelegateCannotBeNull);

            var resultAndOptionsAndVerbInstance = DoParseArgumentsVerbs<T>(args);

            var result = resultAndOptionsAndVerbInstance.Item1;
            var options = resultAndOptionsAndVerbInstance.Item2;
            var verbInstance = resultAndOptionsAndVerbInstance.Item3;

            HandleDynamicAutoBuild(options);

            //TODO: evaluate mutually activation of delegates

            onVerbCommand(args.FirstOrDefault() ?? string.Empty, verbInstance);

            if (!result)
            {
                onFail();
            }

            return options;
        }

        /// <summary>
        /// Frees resources owned by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By design")]
        internal static object InternalGetVerbOptionsInstanceByName(string verb, object target, out bool found)
        {
            found = false;
            if (string.IsNullOrEmpty(verb))
            {
                return target;
            }

            var pair = ReflectionHelper.RetrieveOptionProperty<VerbOptionAttribute>(target, verb);
            found = pair != null;
            return found ? pair.Left.GetValue(target, null) : target;
        }

        private static void SetParserStateIfNeeded(object options, IEnumerable<ParsingError> errors)
        {
            if (!options.CanReceiveParserState())
            {
                return;
            }

            var property = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options)[0].Left;

            var parserState = property.GetValue(options, null);
            if (parserState != null)
            {
                if (!(parserState is IParserState))
                {
                    throw new InvalidOperationException(SR.InvalidOperationException_ParserStateInstanceBadApplied);
                }

                if (!(parserState is ParserState))
                {
                    throw new InvalidOperationException(SR.InvalidOperationException_ParserStateInstanceCannotBeNotNull);
                }
            }
            else
            {
                try
                {
                    property.SetValue(options, new ParserState(), null);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(SR.InvalidOperationException_ParserStateInstanceBadApplied, ex);
                }
            }

            var state = (IParserState)property.GetValue(options, null);

            foreach (var error in errors)
            {
                state.Errors.Add(error);
            }
        }

        private static StringComparison GetStringComparison(ParserSettings settings)
        {
            return settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        }

        private Tuple<bool, T> DoParseArguments<T>(string[] args)
            where T : new()
        {
            var options = new T();
            var pair = ReflectionHelper.RetrieveMethod<HelpOptionAttribute>(options);
            var helpWriter = _settings.HelpWriter;

            // TODO: refactoring following query in TargetCapabilitiesExtensions
            if (pair != null && helpWriter != null)
            {
                if (ParseHelp(args, pair.Right))
                {
                    DisplayHelpText(options, pair, helpWriter);
                    return new Tuple<bool, T>(false, options);
                }

                var optionsAndResult= this.DoParseArgumentsCore(args, options);
                var result = optionsAndResult.Item1;
                options = optionsAndResult.Item2;

                if (!result)
                {
                    DisplayHelpText(new T(), pair, helpWriter);
                    return new Tuple<bool, T>(false, options);
                }
            }

            return DoParseArgumentsCore(args, options);
        }

        private static void DisplayHelpText<T>(T options, Pair<MethodInfo, HelpOptionAttribute> pair, TextWriter helpWriter)
            where T : new()
        {
            string helpText;
            HelpOptionAttribute.InvokeMethod(options, pair, out helpText);
            helpWriter.Write(helpText);
        }

        private Tuple<bool,T> DoParseArgumentsCore<T>(string[] args, T options)
            where T : new()
        {
            var hadError = false;
            var optionMap = OptionMap.Create(options, _settings);
            optionMap.SetDefaults();
            var valueMapper = new ValueMapper(options, _settings.ParsingCulture);

            var arguments = new StringArrayEnumerator(args);
            while (arguments.MoveNext())
            {
                var argument = arguments.Current;
                if (string.IsNullOrEmpty(argument))
                {
                    continue;
                }

                var parser = ArgumentParser.Create(argument, _settings.IgnoreUnknownArguments);
                if (parser != null)
                {
                    var result = parser.Parse(arguments, optionMap, options);
                    if ((result & PresentParserState.Failure) == PresentParserState.Failure)
                    {
                        SetParserStateIfNeeded(options, parser.PostParsingState);
                        hadError = true;
                        continue;
                    }

                    if ((result & PresentParserState.MoveOnNextElement) == PresentParserState.MoveOnNextElement)
                    {
                        arguments.MoveNext();
                    }
                }
                else if (valueMapper.CanReceiveValues)
                {
                    if (!valueMapper.MapValueItem(argument))
                    {
                        hadError = true;
                    }
                }
            }

            hadError |= !optionMap.EnforceRules();

            //return !hadError ? options : default(T);
            return new Tuple<bool, T>(!hadError, options);
        }

        private Tuple<bool, T, object> DoParseArgumentsVerbs<T>(string[] args)
            where T : new()
        {
            var options = new T();

            var verbs = ReflectionHelper.RetrievePropertyList<VerbOptionAttribute>(options);
            var helpInfo = ReflectionHelper.RetrieveMethod<HelpVerbOptionAttribute>(options);

            if (args.Length == 0)
            {
                if (helpInfo != null || _settings.HelpWriter != null)
                {
                    DisplayHelpVerbText(options, helpInfo, null);
                }

                return new Tuple<bool, T, object>(false, options, null);
            }

            var optionMap = OptionMap.Create(options, verbs, _settings);

            if (TryParseHelpVerb(args, options, helpInfo, optionMap))
            {
                return new Tuple<bool, T, object>(false, options, null);
            }

            var verbOption = optionMap[args.First()];

            // User invoked a bad verb name
            if (verbOption == null)
            {
                if (helpInfo != null)
                {
                    DisplayHelpVerbText(options, helpInfo, null);
                }

                return new Tuple<bool, T, object>(false, options, null);
            }

            var verbInstance = verbOption.GetValue(options);
            if (verbInstance == null)
            {
                // Developer has not provided a default value and did not assign an instance
                verbInstance = verbOption.CreateInstance(options);
            }

            verbInstance = DoParseArgumentsCore(args.Skip(1).ToArray(), verbInstance);
            if (verbInstance == null && helpInfo != null)
            {
                // Particular verb parsing failed, we try to print its help
                DisplayHelpVerbText(options, helpInfo, args.First());
            }

            return new Tuple<bool, T, object>(true, options, verbInstance);
        }

        private bool ParseHelp(string[] args, HelpOptionAttribute helpOption)
        {
            var caseSensitive = _settings.CaseSensitive;
            foreach (var arg in args)
            {
                if (helpOption.ShortName != null)
                {
                    if (ArgumentParser.CompareShort(arg, helpOption.ShortName, caseSensitive))
                    {
                        return true;
                    }
                }

                if (string.IsNullOrEmpty(helpOption.LongName))
                {
                    continue;
                }

                if (ArgumentParser.CompareLong(arg, helpOption.LongName, caseSensitive))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryParseHelpVerb<T>(string[] args, T options, Pair<MethodInfo, HelpVerbOptionAttribute> helpInfo, OptionMap optionMap)
            where T : new()
        {
            var helpWriter = _settings.HelpWriter;
            if (helpInfo != null && helpWriter != null)
            {
                if (string.Compare(args[0], helpInfo.Right.LongName, GetStringComparison(_settings)) == 0)
                {
                    // User explicitly requested help
                    var verb = args.FirstOrDefault();
                    if (verb != null)
                    {
                        var verbOption = optionMap[verb];
                        if (verbOption != null)
                        {
                            if (verbOption.GetValue(options) == null)
                            {
                                // We need to create an instance also to render help
                                verbOption.CreateInstance(options);
                            }
                        }
                    }

                    DisplayHelpVerbText(options, helpInfo, verb);
                    return true;
                }
            }

            return false;
        }

        private void DisplayHelpVerbText<T>(T options, Pair<MethodInfo, HelpVerbOptionAttribute> helpInfo, string verb)
            where T : new()
        {
            string helpText;
            if (verb == null)
            {
                HelpVerbOptionAttribute.InvokeMethod(options, helpInfo, null, out helpText);
            }
            else
            {
                HelpVerbOptionAttribute.InvokeMethod(options, helpInfo, verb, out helpText);
            }

            if (_settings.HelpWriter != null)
            {
                _settings.HelpWriter.Write(helpText);
            }
        }

        private void InvokeAutoBuildIfNeeded<T>(T options)
            where T : new()
        {
            if (_settings.HelpWriter == null ||
                options.HasHelp() ||
                options.HasVerbHelp())
            {
                return;
            }

            // We print help text for the user
            _settings.HelpWriter.Write(
                HelpText.AutoBuild(
                    options,
                    current => HelpText.DefaultParsingErrorsHandler(options, current),
                    options.HasVerbs()));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_settings != null)
                {
                    _settings.Dispose();
                }

                _disposed = true;
            }
        }
    }
}