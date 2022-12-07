using LogicStorage.Dtos.ReplayList;
using LogicStorage.Dtos.Track;
using System.Collections.Generic;

namespace LogicStorage.Utils
{
    public static class URLHelper
    {
        private const string _tmnf = "http://tmnforever.tm-exchange.com";
        private const string _tmuf = "http://united.tm-exchange.com";
        private const string _nations = "http://nations.tm-exchange.com";

        public static ApiTypeEnum ApiTypeMapper(string httpResponse)
        {
            if (httpResponse.Contains(_tmnf))
                return ApiTypeEnum.TMNF;
            else if(httpResponse.Contains(_tmuf))
                return ApiTypeEnum.TMUF;
            else
                return ApiTypeEnum.NATIONS;
        }

        public static string ApiTypeToReplayUrlDomain(ApiTypeEnum apiType)
        {
            if (apiType.Equals(ApiTypeEnum.TMNF))
                return _tmnf;
            else if (apiType.Equals(ApiTypeEnum.TMUF))
                return _tmuf;
            else
                return _nations;
        }

        public static List<string> GetAllUrlDomains()
        {
            return new List<string>()
            {
                _tmnf,
                _tmuf,
                _nations
            };
        }

        public static string GetXasecoRequestUrl(string uid)
        {
            return $"https://www.xaseco.org/uidfinder.php?uid={uid}";
        }

        public static string GetTopReplayUrl(TrackAndSourceDto trackData)
        {
            return $"{ApiTypeToReplayUrlDomain(trackData.Source)}/api/replays?trackId={trackData.TrackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt";
        }

        public static string GetDownloadUrl(ReplayDto replayData)
        {
            return $"{ApiTypeToReplayUrlDomain(replayData.Source)}/recordgbx/{replayData.ReplayId}";
        }

        public static string GetSearchByTrackNameUrl(string domain, string trackName)
        {
            return $"{domain}/api/tracks?name={trackName}&count=40&fields=TrackId%2CTrackName%2CAuthors%5B%5D%2CTags%5B%5D%2CAuthorTime%2CRoutes%2CDifficulty%2CEnvironment%2CCar%2CPrimaryType%2CMood%2CAwards%2CHasThumbnail%2CImages%5B%5D%2CIsPublic%2CWRReplay.User.UserId%2CWRReplay.User.Name%2CWRReplay.ReplayTime%2CWRReplay.ReplayScore%2CReplayType%2CUploader.UserId%2CUploader.Name";
        }
    }
}
