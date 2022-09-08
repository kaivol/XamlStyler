// (c) Xavalon. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Xavalon.XamlStyler.Extensions;
using Xavalon.XamlStyler.MarkupExtensions.Parser;
using Xavalon.XamlStyler.Options;

namespace Xavalon.XamlStyler.MarkupExtensions.Formatter
{
    public class MarkupExtensionFormatter
    {
        private readonly IList<string> singleLineTypes;
        private readonly bool keepFirstMarkupExtensionArgumentOnSameLine;
        private readonly int markupExtensionIndentation;
        private readonly int indentSize;

        public MarkupExtensionFormatter(IStylerOptions options)
        {
            this.singleLineTypes = options.NoNewLineMarkupExtensions.ToList();
            this.keepFirstMarkupExtensionArgumentOnSameLine = options.KeepFirstMarkupExtensionArgumentOnSameLine;
            this.markupExtensionIndentation = options.MarkupExtensionIndentation;
            this.indentSize = options.IndentSize;
        }

        /// <summary>
        /// Format markup extension and return elements as formatted lines with "local" indention.
        /// Indention from previous element/attribute/tags must be applied separately
        /// </summary>
        /// <param name="markupExtension"></param>
        /// <param name="prefixLength"></param>
        /// <param name="singleLine"></param>
        /// <returns></returns>
        public IEnumerable<string> Format(
            MarkupExtension markupExtension, 
            int prefixLength = 0,
            bool singleLine = false)
        {
            if (singleLine || this.singleLineTypes.Contains(markupExtension.TypeName)) 
            {
                if (!markupExtension.Arguments.Any()) 
                {
                    yield return $"{{{markupExtension.TypeName}}}";
                }
                else 
                {
                    var inner = string.Join(", ", markupExtension.Arguments.Select(argument => 
                        string.Join("", this.FormatArgument(argument, singleLine: true)
                    )));
                    yield return $"{{{markupExtension.TypeName} {inner}}}";
                }
            }
            else 
            {
                var indent = this.GetAttributeIndentationString(markupExtension.TypeName.Length + 2 + prefixLength);

                using (var enumerator = markupExtension.Arguments
                           .Select(argument => this.FormatArgument(argument))
                           .GetEnumerator()) {
                    
                    var queued = "{" + markupExtension.TypeName;

                    if (enumerator.MoveNext()) 
                    {
                        using (var inner = enumerator.Current.GetEnumerator()) 
                        {
                            if (inner.MoveNext()) 
                            {
                                if (keepFirstMarkupExtensionArgumentOnSameLine) 
                                {
                                    queued += " " + inner.Current;
                                }
                                else {
                                    yield return queued;
                                    queued = indent + inner.Current;
                                }
                            }

                            while (inner.MoveNext()) 
                            {
                                yield return queued;
                                queued = indent + inner.Current;
                            }
                        }
                    }

                    while (enumerator.MoveNext()) 
                    {
                        using (var inner = enumerator.Current.GetEnumerator()) 
                        {
                            if (inner.MoveNext()) 
                            {
                                yield return queued + ",";
                                queued = indent + inner.Current;
                            }

                            while (inner.MoveNext()) 
                            {
                                yield return queued;
                                queued = indent + inner.Current;
                            }
                        }
                    }

                    yield return queued + "}";
                }
            }
        }

        /// <summary>
        /// Format markup extension on a single line.
        /// </summary>
        /// <param name="markupExtension"></param>
        /// <returns></returns>
        public string FormatSingleLine(MarkupExtension markupExtension)
        {
            return this.Format(markupExtension, singleLine: true).Single();
        }

        private string GetAttributeIndentationString(int prefixLength) {
            if (this.markupExtensionIndentation == -1 && this.keepFirstMarkupExtensionArgumentOnSameLine)
                return new string(' ', prefixLength);
            if (this.markupExtensionIndentation > 0)
                return new string(' ', markupExtensionIndentation);
            return new string(' ', indentSize);
        }

        private IEnumerable<string> FormatArgument(Argument argument, bool singleLine = false) {
            switch (argument) {
                case NamedArgument n:
                    return this.FormatNamedArgument(n, singleLine: singleLine);
                case PositionalArgument p:
                    return this.FormatValue(p.Value, singleLine: singleLine);
                default:
                    throw new ArgumentException($"Unhandled type {argument.GetType().FullName}", nameof(argument));
            }
        }

        private IEnumerable<string> FormatNamedArgument(NamedArgument namedArgument, bool singleLine = false) 
        {
            using (var iter = this.FormatValue(
                           namedArgument.Value,
                           prefixLength: namedArgument.Name.Length + 1,
                           singleLine: singleLine
                       )
                       .GetEnumerator()) 
            {
                if (iter.MoveNext()) yield return $"{namedArgument.Name}={iter.Current}";
                while (iter.MoveNext()) yield return iter.Current;
            }
        }

        private IEnumerable<string> FormatValue(Value value, int prefixLength = 0, bool singleLine = false) {
            switch (value) {
                case LiteralValue l:
                    return new[] { l.Value };
                case MarkupExtension e:
                    return this.Format(e, prefixLength: prefixLength, singleLine: singleLine);
                default:
                    throw new ArgumentException($"Unhandled type {value.GetType().FullName}", nameof(value));
            }
        }
    }
}