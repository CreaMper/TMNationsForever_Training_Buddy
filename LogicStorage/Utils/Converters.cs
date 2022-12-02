﻿using LogicStorage.Dtos.TrackData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogicStorage.Utils
{
    public class Converters
    {
        public static string ConvertTrackNameToQuery(string trackName)
        {
            var convertRule = new List<List<string>>()
            {
                new List<string>(){ " ", "%20" },
                new List<string>(){ "!", "%21" },
                new List<string>(){ "#", "%23" },
                new List<string>(){ "$", "%24" },
                new List<string>(){ "%", "%25" },
                new List<string>(){ "&", "%26" },
                new List<string>(){ "'", "%27" },
                new List<string>(){ "(", "%28" },
                new List<string>(){ ")", "%29" },
                new List<string>(){ "*", "%2A" },
                new List<string>(){ "+", "%2B" },
                new List<string>(){ ",", "%2C" },
                new List<string>(){ "/", "%2F" },
                new List<string>(){ ":", "%3A" },
                new List<string>(){ ";", "%3B" },
                new List<string>(){ "=", "%3D" },
                new List<string>(){ "?", "%3F" },
                new List<string>(){ "@", "%40" },
                new List<string>(){ "[", "%5B" },
                new List<string>(){ "]", "%5D" },
            };

            var convertedTrackName = "";
            foreach (var character in trackName)
            {
                var characterToChange = convertRule.FirstOrDefault(x => x.Any(y => y[0].Equals(character)));
                if (characterToChange == null)
                {
                    convertedTrackName += character;
                    continue;
                }

                convertedTrackName += characterToChange[1];
            }

            return convertedTrackName;
        }

        public static ApiTypeEnum ApiTypeConverter(string apiCall)
        {
            if (apiCall.Contains("tmuf.exchange"))
                return ApiTypeEnum.TMUF;
            else if (apiCall.Contains("tmnf.exchange"))
                return ApiTypeEnum.TMNF;
            else if (apiCall.Contains("nations.tm-exchange"))
                return ApiTypeEnum.TMUF;

            return ApiTypeEnum.TMNF;
        }

        public static string TrackNameConverter(string name)
        {
            var specialCharToFilter = new List<string>()
            {
                "$w",
                "$n",
                "$o",
                "$i",
                "$t",
                "$s",
                "$g",
                "$z"
            };

            foreach (var item in specialCharToFilter)
                name = name.Replace(item, "");

            if (name.Contains("$$"))
                name = name.Replace("$$", "$");

            var splittedName = name.Split("$", StringSplitOptions.RemoveEmptyEntries);
            if (splittedName.Count() == 1)
                return name;

            var parsedName = "";
            foreach (var split in splittedName)
            {
                var parseColor = $"{split[0]}{split[1]}{split[2]}";
                if (IsHex(parseColor))
                    split.Remove(0, 2);

                parsedName += split;
            }

            for (int i = 0; i < splittedName.Count(); i++)
            {
                var parseColor = $"{splittedName[i][0]}{splittedName[i][1]}{splittedName[i][2]}";
                if (IsHex(parseColor))
                    splittedName[i] = splittedName[i].Remove(0, 3);
            }

            var gluedName = "";

            foreach (var item in splittedName)
                gluedName += item;

            return gluedName;
        }

        private static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = (c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F');

                if (!isHex)
                    return false;
            }
            return true;
        }

        public static TrackDataDto TrackDataConverter(string[] dataArray)
        {
            return new TrackDataDto()
            {
                UID = dataArray[0],
                TrackName = dataArray[1],
                AuthorName = dataArray[2]
            };
        }

        public static string BytesToStringConverter(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
