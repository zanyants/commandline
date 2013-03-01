﻿using CommandLine.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace CommandLine.Tests.Unit.Attributes
{
    /// <summary>
    /// [Enhancement] https://github.com/gsscoder/commandline/issues/33
    /// </summary>
    public class Attribute_ValueOption_Fixture : ParserBaseFixture
    {
        [Fact]
        public void Index_implicit_by_declaration_order()
        {
            var args = "foo bar".Split();
            var result = true;

            var options = CommandLine.Parser.Default.ParseArguments<OptionsWithValueOptionImplicitIndex>(
                args, () => { result = false; });

            result.Should().BeTrue();
            options.Should().NotBeNull();
            options.A.ShouldBeEquivalentTo("foo");
            options.B.ShouldBeEquivalentTo("bar");
            options.C.Should().BeNull();
        }

        [Fact]
        public void Index_explicitly_set_on_value_option()
        {
            var args = "foo bar".Split();
            var result = true;

            var options = CommandLine.Parser.Default.ParseArguments<OptionsWithValueOptionExplicitIndex>
                (args, () => { result = false; });

            result.Should().BeTrue();
            options.Should().NotBeNull();
            options.A.Should().BeNull();
            options.B.ShouldBeEquivalentTo("bar");
            options.C.ShouldBeEquivalentTo("foo");
        }

        [Fact]
        public void Value_option_attribute_isolates_non_option_values()
        {
            var parser = new CommandLine.Parser();
            var result = true;
            var options = parser.ParseArguments<SimpleOptionsWithValueOption>(
                new string[] { "--switch", "file.ext", "1000", "0.1234", "-s", "out.ext" }, () => { result = false; });

            result.Should().BeTrue();

            options.BooleanValue.Should().BeTrue();
            options.StringItem.Should().Be("file.ext");
            options.IntegerItem.Should().Be(1000);
            options.NullableDoubleItem.Should().Be(0.1234D);
            options.StringValue.Should().Be("out.ext");
        }

        [Fact]
        public void Value_option_attribute_values_are_not_mandatory()
        {
            var parser = new CommandLine.Parser();
            var result = true;
            var options = parser.ParseArguments<SimpleOptionsWithValueOption>(
                new string[] { "--switch" }, () => { result = false; });

            result.Should().BeTrue();

            options.BooleanValue.Should().BeTrue();
            options.StringItem.Should().BeNull();
            options.IntegerItem.Should().Be(0);
            options.NullableDoubleItem.Should().NotHaveValue();
        }

        [Fact]
        public void Value_option_takes_precedence_on_value_list_regardless_declaration_order()
        {
            var parser = new CommandLine.Parser();
            var result = true;
            var options = parser.ParseArguments<SimpleOptionsWithValueOptionAndValueList>(
                new string[] { "ofvalueoption", "-1234", "4321", "forvaluelist1", "forvaluelist2", "forvaluelist3" }, () => { result = false; });

            result.Should().BeTrue();

            options.StringItem.Should().Be("ofvalueoption");
            options.NullableInteger.Should().Be(-1234);
            options.UnsignedIntegerItem.Should().Be(4321U);
            options.Items[0].Should().Be("forvaluelist1");
            options.Items[1].Should().Be("forvaluelist2");
            options.Items[2].Should().Be("forvaluelist3");
        }

        [Fact]
        public void Between_value_options_order_matters()
        {
            var parser = new CommandLine.Parser();
            var result = true;
            var options = parser.ParseArguments<SimpleOptionsWithValueOptionAndValueList>(
                new string[] { "4321", "ofvalueoption", "-1234", "forvaluelist1", "forvaluelist2", "forvaluelist3" }, () => { result = false; });

            result.Should().BeFalse();
        }
    }
}