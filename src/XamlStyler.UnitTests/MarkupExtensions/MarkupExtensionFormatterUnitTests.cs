// (c) Xavalon. All rights reserved.

using System.Linq;
using NUnit.Framework;
using Xavalon.XamlStyler.MarkupExtensions.Formatter;
using Xavalon.XamlStyler.MarkupExtensions.Parser;
using Xavalon.XamlStyler.Options;

namespace Xavalon.XamlStyler.UnitTests.MarkupExtensions
{
    [TestFixture]
    public class MarkupExtensionFormatterUnitTests
    {
        private MarkupExtensionParser parser;
        
        private static readonly IStylerOptions OptionsDefault = new StylerOptions {
            NoNewLineMarkupExtensions = "",
            KeepFirstMarkupExtensionArgumentOnSameLine = false,
            MarkupExtensionIndentation = 0,
        };
        
        private static readonly IStylerOptions OptionsKeepFirstLine = new StylerOptions {
            NoNewLineMarkupExtensions = "",
            KeepFirstMarkupExtensionArgumentOnSameLine = true,
            MarkupExtensionIndentation = 0,
        };
        
        private static readonly IStylerOptions OptionsKeepFirstLineAndAlign = new StylerOptions {
            NoNewLineMarkupExtensions = "",
            KeepFirstMarkupExtensionArgumentOnSameLine = true,
            MarkupExtensionIndentation = -1,
        };

        private static readonly IStylerOptions[] Options = {
            OptionsDefault,
            OptionsKeepFirstLine,
            OptionsKeepFirstLineAndAlign,
        };

        [SetUp]
        public void Setup()
        {
            this.parser = new MarkupExtensionParser();
        }

        private static MarkupExtensionFormatter Formatter(IStylerOptions options) {
            return new MarkupExtensionFormatter(options);
        }

        [TestCase("{Hello}", new[] {
            "{Hello}",
            "{Hello}",
            "{Hello}",
        })]
        [TestCase("{Hello world}", new[] {
@"
{Hello
    world}",
@"
{Hello world}",
@"
{Hello world}",
        })]
        [TestCase("{Hello big world}", new[] {
@"
{Hello
    big world}",
@"
{Hello big world}",
@"
{Hello big world}",
        })]
        [TestCase("{Hello big,world}", new[] {
@"
{Hello
    big,
    world}",
@"
{Hello big,
    world}",
@"
{Hello big,
       world}",
        })]
        [TestCase("{Hello big=world}", new[] {
@"
{Hello
    big=world}",
@"
{Hello big=world}",
@"
{Hello big=world}",
        })]
        [TestCase("{The Answer,is=42}", new[] {
@"
{The
    Answer,
    is=42}",
@"
{The Answer,
    is=42}",
@"
{The Answer,
     is=42}",
        })]
        [TestCase("{The Answer , is = 42}", new[] {
@"
{The
    Answer,
    is=42}",
@"
{The Answer,
    is=42}",
@"
{The Answer,
     is=42}",
        })]
        [TestCase("{A {x:B c}}", new[] {
@"
{A
    {x:B
        c}}",
@"
{A {x:B c}}",
@"
{A {x:B c}}",
        })]
        [TestCase("{A {x:B c}, D={x:E f}}", new[] {
@"
{A
    {x:B
        c},
    D={x:E
        f}}",
@"
{A {x:B c},
    D={x:E f}}",
@"
{A {x:B c},
   D={x:E f}}",
        })]
        [TestCase("{A B, C={D E,F={G H}}}", new[] {
@"
{A
    B,
    C={D
        E,
        F={G
            H}}}",
@"
{A B,
    C={D E,
        F={G H}}}",
@"
{A B,
   C={D E,
        F={G H}}}",
        })]
        [TestCase(
@"{Binding {}Title, RelativeSource={RelativeSource FindAncestor,AncestorType ={x:Type Page}},StringFormat={}{0}Now{{0}} - {0}}",
            new[] {
@"
{Binding
    {}Title,
    RelativeSource={RelativeSource
        FindAncestor,
        AncestorType={x:Type
            Page}},
    StringFormat={}{0}Now{{0}} - {0}}",
@"
{Binding {}Title,
    RelativeSource={RelativeSource FindAncestor,
        AncestorType={x:Type Page}},
    StringFormat={}{0}Now{{0}} - {0}}",
@"
{Binding {}Title,
         RelativeSource={RelativeSource FindAncestor,
                                        AncestorType={x:Type Page}},
         StringFormat={}{0}Now{{0}} - {0}}",
})]
        public void TestFormatter(string sourceText, string[] expected)
        {
            MarkupExtension markupExtension;
            Assert.True(this.parser.TryParse(sourceText.Trim(), out markupExtension));

            Assert.AreEqual(Options.Length, expected.Length);
            foreach (var (option, output) in Options.Zip(expected)) {
                var result = Formatter(option).Format(markupExtension);
                Assert.AreEqual(output.TrimStart(), string.Join('\n', result));
            }
        }
        
         [TestCase("{Hello}", "{Hello}")]
         [TestCase("{Hello world}", "{Hello world}")]
         [TestCase("{Hello big world}", "{Hello big world}")]
         [TestCase("{Hello big,world}", "{Hello big, world}")]
         [TestCase("{Hello big=world}", "{Hello big=world}")]
         [TestCase("{The Answer,is=42}", "{The Answer, is=42}")]
         [TestCase("{The Answer , is = 42}", "{The Answer, is=42}")]
         [TestCase("{A {x:B c}}", "{A {x:B c}}")]
         [TestCase("{A {x:B c}, D={x:E f}}", "{A {x:B c}, D={x:E f}}")]
         [TestCase("{A B, C={D E,F={G H}}}", "{A B, C={D E, F={G H}}}")]
         public void TestSingleLineFormatter(string sourceText, string expected)
         {
             MarkupExtension markupExtension;
             Assert.True(this.parser.TryParse(sourceText, out markupExtension));

             var result = Formatter(OptionsDefault).FormatSingleLine(markupExtension);
             Assert.That(result, Is.EqualTo(expected));
         }
    }
}