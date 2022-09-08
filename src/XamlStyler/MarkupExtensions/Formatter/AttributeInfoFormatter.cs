// (c) Xavalon. All rights reserved.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xavalon.XamlStyler.Extensions;
using Xavalon.XamlStyler.Model;
using Xavalon.XamlStyler.Options;
using Xavalon.XamlStyler.Services;

namespace Xavalon.XamlStyler.MarkupExtensions.Formatter
{
    public class AttributeInfoFormatter
    {
        private readonly MarkupExtensionFormatter formatter;
        private readonly IndentService indentService;

        public AttributeInfoFormatter(MarkupExtensionFormatter formatter, IndentService indentService)
        {
            this.formatter = formatter;
            this.indentService = indentService;
        }

        /// <summary>
        /// Handles markup extension value in style as:
        /// XyzAttribute="{XyzMarkup value1,
        ///                          value2,
        ///                          key1=value1,
        ///                          key2=value2}"
        /// </summary>
        /// <param name="attrInfo"></param>
        /// <param name="baseIndentationString"></param>
        /// <returns></returns>
        public string ToMultiLineString(AttributeInfo attrInfo, string baseIndentationString)
        {
            if (!attrInfo.IsMarkupExtension)
            {
                throw new ArgumentException(
                    "AttributeInfo shall have a markup extension value.",
                    MethodBase.GetCurrentMethod().GetParameters()[0].Name);
            }

            var lines = this.formatter.Format(attrInfo.MarkupExtension, attrInfo.Name.Length + 2);

            var buffer = new StringBuilder();
            buffer.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}", attrInfo.Name, lines.First());
            foreach (var line in lines.Skip(1))
            {
                buffer.AppendLine();
                buffer.Append(this.indentService.Normalize(baseIndentationString + line));
            }

            buffer.Append('"');
            return buffer.ToString();
        }

        /// <summary>
        /// Single line value line in style as:
        /// attribute_name="attribute_value"
        /// </summary>
        /// <param name="attrInfo"></param>
        /// <returns></returns>
        public string ToSingleLineString(AttributeInfo attrInfo, XamlLanguageOptions xamlLanguageOptions)
        {
            var valuePart = attrInfo.IsMarkupExtension
                ? this.formatter.FormatSingleLine(attrInfo.MarkupExtension)
                : attrInfo.Value.ToXmlEncodedString(xamlLanguageOptions.UnescapedAttributeCharacters);

            return $"{attrInfo.Name}=\"{valuePart}\"";
        }
    }
}