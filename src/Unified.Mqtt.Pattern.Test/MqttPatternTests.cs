using NUnit.Framework;
using System.Collections.Generic;

namespace Unified.Mqtt.Pattern.Test
{
    public class MqttPatternTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void matches_supports_patterns_with_no_wildcards()
        {
            if (MqttPattern.Matches("foo/bar/baz", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_doesnt_match_different_topics()
        {
            if (MqttPattern.Matches("foo/bar/baz", "baz/bar/foo") == false)
            {
                Assert.Pass("Didn't match topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_hash_at_the_beginning()
        {
            if (MqttPattern.Matches("#", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_hash_at_the_end()
        {
            if (MqttPattern.Matches("foo/#", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_hash_at_the_end_and_topic_has_no_children()
        {
            if (MqttPattern.Matches("foo/bar/#", "foo/bar") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_doesnt_support_hash_wildcards_with_more_after_them()
        {
            if (MqttPattern.Matches("#/bar/baz", "foo/bar/baz") == false)
            {
                Assert.Pass("Didn't match topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_plus_at_the_beginning()
        {
            if (MqttPattern.Matches("+/bar/baz", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_plus_at_the_end()
        {
            if (MqttPattern.Matches("foo/bar/+", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_with_plus_in_the_middle()
        {
            if (MqttPattern.Matches("foo/+/baz", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_patterns_multiple_wildcards()
        {
            if (MqttPattern.Matches("foo/+/#", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_named_wildcards()
        {
            if (MqttPattern.Matches("foo/+something/#else", "foo/bar/baz") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_leading_slashes()
        {
            if (MqttPattern.Matches("/foo/bar", "/foo/bar") == true)
            {
                Assert.Pass("Matched topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void matches_supports_leading_slashes_but_different_topic()
        {
            if (MqttPattern.Matches("/foo/bar", "/bar/foo") == false)
            {
                Assert.Pass("Didn't match invalid topic");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void extract_returns_empty_object_if_theres_nothing_to_extract()
        {
            if (MqttPattern.Extract("foo/bar/baz", "foo/bar/baz").Count == 0)
            {
                Assert.Pass("Extracted empty object");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void extract_returns_empty_object_if_wildcards_dont_have_label()
        {
            if (MqttPattern.Extract("foo/+/#", "foo/bar/baz").Count == 0)
            {
                Assert.Pass("Extracted empty object");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void extract_returns_object_with_an_array_for_hash_wildcard()
        {
            var expected = new Dictionary<string, string[]>()
            {
                { "something", new [] { "bar", "baz" } },
            };

            var actual = MqttPattern.Extract("foo/#something", "foo/bar/baz");

            if (DictionaryContainSameData(expected, actual) == true)
            {
                Assert.Pass("Extracted object: " + actual.ToString());
            }
            else
                Assert.Fail();
        }

        [Test]
        public void extract_returns_object_with_a_string_for_plus_wildcard()
        {
            var expected = new Dictionary<string, string[]>()
            {
                { "hello", new [] { "bar" } },
                { "world", new [] { "baz" } }
            };

            var actual = MqttPattern.Extract("foo/+hello/+world", "foo/bar/baz");

            if (DictionaryContainSameData(expected, actual) == true)
            {
                Assert.Pass("Extracted object: " + actual.ToString());
            }
            else
                Assert.Fail();
        }

        [Test]
        public void extract_parses_params_from_all_wildcards()
        {
            var expected = new Dictionary<string, string[]>()
            {
                { "hello", new [] { "foo" } },
                { "world", new [] { "bar" } },
                { "wow", new [] { "baz", "fizz" } }
            };

            var actual = MqttPattern.Extract("+hello/+world/#wow", "foo/bar/baz/fizz");

            if (DictionaryContainSameData(expected, actual) == true)
            {
                Assert.Pass("Extracted object: " + actual.ToString());
            }
            else
                Assert.Fail();
        }

        [Test]
        public void exec_returns_null_if_it_doesnt_match()
        {
            if (MqttPattern.Exec("hello/world", "foo/bar/baz").Count == 0)
            {
                Assert.Pass("Got empty collection");
            }
            else
                Assert.Fail();
        }

        [Test]
        public void exec_returns_params_if_they_can_be_parsed()
        {
            var actual = MqttPattern.Exec("foo/+hello/#world", "foo/bar/baz");

            var expected = new Dictionary<string, string[]>()
            {
                { "hello", new [] {"bar" } },
                { "world", new [] { "baz" } }
            };

            if (DictionaryContainSameData(actual, expected) == true)
            {
                Assert.Pass("Extracted object: " + actual.ToString());
            }
            else
                Assert.Fail();
        }

        [Test]
        public void fill_fills_in_pattern_with_both_types_of_wildcards()
        {
            const string expected = "foo/Hello/the/world/wow";
            var actual = MqttPattern.Fill(
                            "foo/+hello/#world",
                            new Dictionary<string, string[]>()
                            {
                                        { "hello", new [] { "Hello" } },
                                        { "world", new [] { "the", "world", "wow" } }
                            });

            if (actual == expected)
            {
                Assert.Pass("String is filled, got " + actual);
            }
            else
                Assert.Fail("Got string" + actual);
        }


        [Test]
        public void fill_fills_in_missing_plus_params_with_empty()
        {
            const string expected = "foo/undefined";
            var actual = MqttPattern.Fill(
                            "foo/+hello",
                            new Dictionary<string, string[]>()
                            { });

            if (actual == expected)
            {
                Assert.Pass("String is filled, got " + actual);
            }
            else
                Assert.Fail("Got string" + actual);
        }


        [Test]
        public void fill_ignores_empty_hash_params()
        {
            const string expected = "foo";
            var actual = MqttPattern.Fill(
                    "foo/#hello",
                    new Dictionary<string, string[]>()
                    { });

            if (actual == expected)
            {
                Assert.Pass("String is filled, got " + actual);
            }
            else
                Assert.Fail("Got string" + actual);
        }


        [Test]
        public void fill_ignores_non_named_hash_params()
        {
            const string expected = "foo";
            var actual = MqttPattern.Fill(
                    "foo/#",
                    new Dictionary<string, string[]>()
                    { });

            if (actual == expected)
            {
                Assert.Pass("String is filled, got " + actual);
            }
            else
                Assert.Fail("Got string" + actual);
        }

        [Test]
        public void test_fill_uses_undefined_for_non_named_plus_params()
        {
            const string expected = "foo/undefined";
            var actual = MqttPattern.Fill(
                    "foo/+",
                    new Dictionary<string, string[]>()
                    { });

            if (actual == expected)
            {
                Assert.Pass("String is filled, got " + actual);
            }
            else
                Assert.Fail();
        }

        [Test]
        public void clean_removes_parameter_names()
        {
            const string expected = "hello/+/world/#";

            var actual = MqttPattern.Clean("hello/+param1/world/#param2");

            if (actual == expected)
            {
                Assert.Pass("String is cleaned, got " + expected);
            }
            else
                Assert.Fail();
        }

        [Test]
        public void clean_works_when_there_arent_any_parameter_names()
        {
            const string expected = "hello/+/world/#";

            var actual = MqttPattern.Clean("hello/+/world/#");

            if (actual == expected)
            {
                Assert.Pass("String is cleaned, got " + expected);
            }
            else
                Assert.Fail("Got string" + actual);
        }

        bool DictionaryContainSameData(Dictionary<string, string[]> expected, Dictionary<string, string[]> actual)
        {
            return
                new DictionaryComparer<string, string[]>(new ArrayComparer<string>())
                        .Equals(expected, actual);
        }
    }
}