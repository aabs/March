//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


using System.Globalization;
using Common;

namespace MGraphXamlReader
{
    using System;
    using System.Collections.Generic;
    using System.Dataflow;
    using System.IO;
    using System.Linq;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml.Linq;
    using Microsoft.M.Parser;

    public static class DynamicParserExtensions
    {
        public static T Parse<T>(this DynamicParser parser, string input, Dictionary<Identifier, Type> xamlMap)
        {
            return (T)ParseObject(parser, new StringReader(input), null, xamlMap);
        }

        public static T Parse<T>(this DynamicParser parser, string input, Dictionary<Identifier, Type> xamlMap, ErrorReporter reporter)
        {
            return (T)ParseObject(parser, new StringReader(input), reporter, xamlMap);
        }

        public static T Parse<T>(this DynamicParser parser, TextReader input, Dictionary<Identifier, Type> xamlMap)
        {
            return (T)ParseObject(parser, input, null, xamlMap);
        }

        public static T Parse<T>(this DynamicParser parser, TextReader input, ErrorReporter errorReporter, Dictionary<Identifier, Type> xamlMap)
        {
            return (T)ParseObject(parser, input, errorReporter, xamlMap);
        }

        public static object ParseObject(this DynamicParser parser, string input, Dictionary<Identifier, Type> xamlMap)
        {
            return ParseObject(parser, new StringReader(input), null, xamlMap);
        }

        public static object ParseObject(this DynamicParser parser, TextReader input, Dictionary<Identifier, Type> xamlMap)
        {
            return ParseObject(parser, input, null, xamlMap);
        }

        public static object ParseObject(this DynamicParser parser, TextReader input, ErrorReporter errorReporter, Dictionary<Identifier, Type> xamlMap)
        {
            return ParseObject(parser, input, errorReporter, xamlMap, null);
        }

        public static object ParseObject(this DynamicParser parser, TextReader input, ErrorReporter errorReporter, Dictionary<Identifier, Type> xamlMap, Dictionary<XNamespace, string> namespaces)
        {
            var xamlReader = ParseToXaml(parser, input, errorReporter, xamlMap, namespaces);
            if (xamlReader == null) { return null; }
            return XamlServices.Load(xamlReader);
        }

        public static XamlReader ParseToXaml(this DynamicParser parser, string input, Dictionary<Identifier, Type> xamlMap)
        {
            return ParseToXaml(parser, new StringReader(input), null, xamlMap, null);
        }

        public static XamlReader ParseToXaml(this DynamicParser parser, string input, Dictionary<Identifier, Type> xamlMap, Dictionary<XNamespace, string> namespaces)
        {
            return ParseToXaml(parser, new StringReader(input), null, xamlMap, namespaces);
        }

        public static XamlReader ParseToXaml(this DynamicParser parser, TextReader input, Dictionary<Identifier, Type> xamlMap)
        {
            return ParseToXaml(parser, input, null, xamlMap, null);
        }

        public static XamlReader ParseToXaml(this DynamicParser parser, TextReader input, Dictionary<Identifier, Type> xamlMap, Dictionary<XNamespace, string> namespaces)
        {
            return ParseToXaml(parser, input, null, xamlMap, namespaces);
        }

        public static XamlReader ParseToXaml(this DynamicParser parser, TextReader input, ErrorReporter errorReporter, Dictionary<Identifier, Type> xamlMap, Dictionary<XNamespace, string> namespaces)
        {
            if (parser == null) { throw new ArgumentNullException("parser"); }
            if (input == null) { throw new ArgumentNullException("input"); }
            errorReporter = errorReporter ?? new ExceptionErrorReporter();
            if (xamlMap == null) { throw new ArgumentNullException("xamlMap"); }


            var result = parser.Parse<object>(null, input, errorReporter);
            if (result == null || errorReporter.HasErrors) { return null; }


            var gb = parser.GraphBuilder;
            var builder = null != gb
                ? gb
                : new System.Dataflow.GraphBuilder();

            return new MGraphXamlReader
            {
                AdditionalNamespaces = namespaces == null
                    ? Enumerable.Empty<Pair<XNamespace, string>>().ToList()
                    : namespaces.Select(kvp => new Pair<XNamespace, string>(kvp.Key, kvp.Value)).ToList(),
                GraphBuilder = builder,
                GraphRoot = result,
                LabelMap = xamlMap,
            };
        }

        internal class ExceptionErrorReporter : ErrorReporter
        {
            public static readonly Logger Logger = new Logger(typeof(ExceptionErrorReporter)); 

            protected override void OnError(ErrorInformation errorInformation)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, errorInformation.Message, errorInformation.Arguments.ToArray());
                Logger.Error("Error while parsing to XAML: {0}.", errorMessage);
//                var eventArgs = errorInformation.ToBuildEventArgs(String.Empty, String.Empty);
//                throw new Exception(String.Format("Exception while parsing to xaml: {0}", eventArgs.ToString()));
            }
        }
    }
}