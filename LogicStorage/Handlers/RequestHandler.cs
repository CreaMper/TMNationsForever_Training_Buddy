using LogicStorage.Dtos.SearchQuery;
using LogicStorage.Dtos.TrackData;
using LogicStorage.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace LogicStorage.Handlers
{
    public class RequestHandler
    {
        private static HttpClient _httpClient { get; set; }

        public RequestHandler()
        {
            _httpClient = new HttpClient();
        }

        public string XasecoCheck(TrackDataDto track)
        {

            var xasecoURL = $"https://www.xaseco.org/uidfinder.php?uid={track.UID}";

            var response = HttpRequestAsStringSync(xasecoURL);
            if (response == null)
            {
                return null;
            }

            if (response.Contains("This UId cannot be found on TMX"))
            {
                return null;
            }

            try
            {
                var splitedContent = response.Split("&id=");
                var split = splitedContent[1].Split("\">TMX");
                var onlyId = split[0];

                return onlyId;
            }
            catch
            {
                return null;
            }
        }

        public string HttpRequestAsStringSync(string url)
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                return null;
        }

        public Stream HttpRequestAsStreamSync(string url)
        {
            var response = _httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStreamAsync().Result;
            else
                return null;
        }

        public bool DownloadReplayUsingXasecoApproach(TrackDataDto trackInfo)
        {
            var trackId = XasecoCheck(trackInfo);
            if (trackId == null)
            {
                return false;
            }

            var download = DownloadReplayFromTMX(trackId);
            if (!download)
            {
                return false;
            }

            return true;
        }

        public bool DownloadReplayUsingTMXApproach(TrackDataDto trackInfo)
        {
            var convertedTrackName = Converters.TrackNameConverter(trackInfo.TrackName);

            var trackId = TMXCheck(convertedTrackName);
            if (trackId == null)
                return false;

            var download = DownloadReplayFromTMX(trackId);
            if (!download)
            {
                return false;
            }

            return true;
        }

        private string TMXCheck(string trackName)
        {
            var queryTrackName = Converters.ConvertTrackNameToQuery(trackName);

            var tmxSearchResult = $"https://tmnf.exchange/api/tracks?name={queryTrackName}&count=40&fields=TrackId%2CTrackName%2CAuthors%5B%5D%2CTags%5B%5D%2CAuthorTime%2CRoutes%2CDifficulty%2CEnvironment%2CCar%2CPrimaryType%2CMood%2CAwards%2CHasThumbnail%2CImages%5B%5D%2CIsPublic%2CWRReplay.User.UserId%2CWRReplay.User.Name%2CWRReplay.ReplayTime%2CWRReplay.ReplayScore%2CReplayType%2CUploader.UserId%2CUploader.Name";

            var response = HttpRequestAsStringSync(tmxSearchResult);
            if (response == null)
                return null;

            var searchDto = JsonConvert.DeserializeObject<SearchDto>(response);

            try
            {
                var trackId = searchDto.Results.FirstOrDefault().TrackId.ToString();
                return trackId;
            }
            catch
            {
                return null;
            }
        }

        public bool DownloadReplayFromTMX(string trackId)
        {
            var apiUrls = new List<string>()
            {
                $"https://tmuf.exchange/api/replays?trackId={trackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt",
                $"https://tmnf.exchange/api/replays?trackId={trackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt",
                $"https://nations.tm-exchange.com/api/replays?trackId={trackId}&best=1&count=10&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CScore%2CTrack.Type%2CPosition%2CIsBest%2CIsLeaderboard%2CReplayAt"
            };

            string response = null;
            ApiTypeEnum apiType = ApiTypeEnum.TMNF;
            foreach (var url in apiUrls)
            {
                response = HttpRequestAsStringSync(url);
                if (response != null)
                {
                    apiType = Converters.ApiTypeConverter(url);
                    break;
                }
            }

            if (response == null)
                return false;

            var trackStats = JsonConvert.DeserializeObject<TrackStatsDto>(response);
            if (trackStats == null)
                return false;

            var topOneReplayId = trackStats.Results.FirstOrDefault().ReplayId;

            var apiDownloadLink = "";
            if (apiType.Equals(ApiTypeEnum.TMNF))
                apiDownloadLink = $"https://tmnf.exchange/recordgbx/{topOneReplayId}";
            else if (apiType.Equals(ApiTypeEnum.TMUF))
                apiDownloadLink = $"https://tmuf.exchange/recordgbx/{topOneReplayId}";
            else if (apiType.Equals(ApiTypeEnum.EXCHANGE))
                apiDownloadLink = $"https://nations.tm-exchange/recordgbx/{topOneReplayId}";

            var packageStream = HttpRequestAsStreamSync(apiDownloadLink);
            if (packageStream == null)
                return false;

            if (File.Exists("replay.gbx"))
                File.Delete("replay.gbx");

            var fileStream = File.Create("replay.gbx");
            packageStream.CopyTo(fileStream);

            packageStream.Close();
            fileStream.Close();

            return true;
        }
    }
}
