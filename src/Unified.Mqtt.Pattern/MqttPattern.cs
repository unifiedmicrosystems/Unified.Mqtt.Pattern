using System;
using System.Collections.Generic;

namespace Unified.Mqtt.Pattern
{
    ///<Summary>
    /// Static class providing mqtt pattern matching functions.
    ///</Summary>
    public static class MqttPattern
    {
        const char Seperator = '/';
        const char Single = '+';
        const char All = '#';
        const string Undefined = "undefined";

        ///<Summary>
        /// Validates that topic fits the pattern and parses out any parameters. If the
        /// topic doesn't match, it returns an empty dictionary.
        ///</Summary>
        public static Dictionary<string, string[]> Exec(string pattern, string topic)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            if (Matches(pattern, topic) == true)
            {
                return Extract(pattern, topic);
            }
            else
                return new Dictionary<string, string[]>();
        }

        ///<Summary>
        /// Validates whether topic fits the pattern. Ignores parameters.
        ///</Summary>
        public static bool Matches(string pattern, string topic)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var patternSegments = pattern.Split(Seperator);
            var topicSegments = topic.Split(Seperator);

            var lastIndex = patternSegments.Length - 1;

            for (var i = 0; i < patternSegments.Length; i++)
            {
                var currentPattern = patternSegments[i];
                var patternChar = string.IsNullOrEmpty(currentPattern) ? (char?)null : currentPattern[0];
                var currentTopic = i < topicSegments.Length ? topicSegments[i] : null;

                if (currentTopic == null && currentPattern == null)
                    continue;

                if (currentTopic == null && string.Compare(currentPattern, All.ToString()) != 0)
                    return false;

                // Only allow # at end
                if (patternChar == All)
                    return i == lastIndex;

                if (patternChar != Single && currentPattern != currentTopic)
                    return false;
            }

            return patternSegments.Length == topicSegments.Length;
        }

        ///<Summary>
        /// Reverse of extract, traverse the pattern and fill in params with keys in an
        /// object. Missing keys for + params are set to undefined. Missing keys for #
        /// params yeid empty strings.
        ///</Summary>
        public static string Fill(string pattern, Dictionary<string, string[]> parameters)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var patternSegments = pattern.Split(Seperator);

            var result = new List<string>();

            for (var i = 0; i < patternSegments.Length; i++)
            {
                var currentPattern = patternSegments[i];
                var patternChar = currentPattern[0];
                var patternParam = currentPattern.Substring(1);
                var gotValidParameter = parameters.TryGetValue(patternParam, out string[] paramValue);

                if (patternChar == All)
                {
                    if (gotValidParameter)
                        result.Add(String.Join(Seperator.ToString(), paramValue));

                    // Since # wildcards are always at the end, break out of the loop
                    break;
                }
                else if (patternChar == Single)
                {
                    // Coerce param into a string, missing params will be undefined
                    if (gotValidParameter)
                        result.Add(paramValue[0]);
                    else
                        result.Add(Undefined);
                }
                else
                {
                    result.Add(currentPattern);
                }
            }
            return string.Join(Seperator.ToString(), result);
        }

        ///<Summary>
        /// Traverses the pattern and attempts to fetch parameters from the topic. Useful
        /// if you know in advance that your topic will be valid and want to extract data.
        /// If the topic doesn't match, or the pattern doesn't contain named wildcards,
        /// returns an empty dictionary. Do not use this for validation.
        ///</Summary>
        public static Dictionary<string, string[]> Extract(string pattern, string topic)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var extracted = new Dictionary<string, string[]>();

            var patternSegments = pattern.Split(Seperator);
            var topicSegments = topic.Split(Seperator);

            var patternLength = patternSegments.Length;

            for (var i = 0; i < patternLength; i++)
            {
                var currentPattern = patternSegments[i];
                var patternChar = currentPattern[0];

                if (currentPattern.Length == 1)
                    continue;

                if (patternChar == All)
                {
                    var count = topicSegments.Length - i; // get remaining items
                    var itemsPastAllWildcard = new string[count];
                    Array.Copy(topicSegments, i, itemsPastAllWildcard, 0, count);
                    extracted[currentPattern.Substring(1)] = itemsPastAllWildcard;
                    break;
                }
                else if (patternChar == Single)
                {
                    extracted[currentPattern.Substring(1)] = new string[] { topicSegments[i] };
                }
            }

            return extracted;
        }

        ///<Summary>
        /// Removes the parameter names from a pattern.
        ///</Summary>
        public static string Clean(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            var patternSegments = pattern.Split(Seperator);
            var patternLength = patternSegments.Length;

            var cleanedSegments = new List<string>();

            for (var i = 0; i < patternLength; i++)
            {
                var currentPattern = patternSegments[i];
                var patternChar = currentPattern[0];

                if (patternChar == All)
                {
                    cleanedSegments.Add(All.ToString());
                }
                else if (patternChar == Single)
                {
                    cleanedSegments.Add(Single.ToString());
                }
                else
                {
                    cleanedSegments.Add(currentPattern);
                }
            }

            return string.Join(Seperator.ToString(), cleanedSegments);
        }

    }
}

