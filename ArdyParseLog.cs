using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Ardy.Tools.ArdyLogParser
{
    static public class ArdyParseLog
    {
        static public object ParseLog(string pattern, string requestBody, List<string> parseOptions)
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            List<string> groupNameCol = new List<string>();
            foreach (string name in regex.GetGroupNames())
            {
                if (name.Length >= 2)
                {
                    groupNameCol.Add(name);
                }
            }

            object result = new object();

            if (parseOptions.Contains("WLSERVER_TIMESTAMPUTC"))
            {
                MatchCollection matchCol = regex.Matches(requestBody);
                result = GetMatches(matchCol, groupNameCol, "RawTime", "yyyy-MM-dd HH:mm:ss.fff");
            }
            else
            {
                MatchCollection matchCol = regex.Matches(requestBody);
                result = GetMatches(matchCol, groupNameCol);
            }
            
            return result;           
        }

        static private object GetMatches(MatchCollection matchCol, List<string> groupNameCol)
        {
            List<object> result = new List<object>();
            foreach (var (m, dict) in from Match match in matchCol
                                        let dict = new Dictionary<string, string>()
                                        select (match, dict))
            {
                for (int i = 0; i < groupNameCol.Count; i++)
                {
                    string groupName = groupNameCol[i];
                    dict.Add(groupName, m.Groups[groupName].Value);
                }
                result.Add(dict);
            }

            return result;
        }

        static private object GetMatches(MatchCollection matchCol, List<string> groupNameCol, string utmField,string dateFormat)
        // Create DateTime based on 'RawTime' field
        // Bypass hassle with processing the datetime pattern used by the default Hyperion WLServerLog format
        {
            List<object> result = new List<object>();
            foreach (var (m, dict, dateTime) in from Match match in matchCol
                                                let dict = new Dictionary<string, string>()                                                
                                                let epoch = long.Parse(match.Groups[utmField].Value)
                                                let dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(epoch)
                                                let dateTime = dateTimeOffset.UtcDateTime
                                                select (match, dict, dateTime))
            {
                dict.Add("TimestampUTC", dateTime.ToString(dateFormat));
                for (int i = 0; i < groupNameCol.Count; i++)
                {
                    string groupName = groupNameCol[i];
                    dict.Add(groupName, m.Groups[groupName].Value);
                }
                result.Add(dict);
            }

            return result;
        }
    }
}
