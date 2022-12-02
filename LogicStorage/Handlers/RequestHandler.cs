using LogicStorage.Dtos;
using LogicStorage.Dtos.SearchQuery;
using LogicStorage.Dtos.TrackData;
using LogicStorage.Utils;
using Newtonsoft.Json;
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

        public TrackIdAndSourceDto GetTrackIdAndSource(TrackDataDto trackData)
        {
            var uidCheck = SearchByUID(trackData.UID);
            if (uidCheck != null)
                return uidCheck;

            var nameCheck = SearchByTrackName(trackData.AuthorName);
            if (nameCheck != null)
                return nameCheck;

            return null;
        }

        public TrackIdAndSourceDto SearchByUID(string uid)
        {
            var xasecoURL = URLHelper.GetXasecoRequestUrl(uid);

            var response = HttpRequestAsStringSync(xasecoURL);
            if (response == null)
                return null;

            if (response.Contains("This UId cannot be found on TMX"))
                return null;

            try
            {
                var contentLeftCut = response.Split("&id=");
                var contentRightCut = contentLeftCut[1].Split("\">TMX");
                var trackId = contentRightCut[0];

                return new TrackIdAndSourceDto() 
                {
                    Source = URLHelper.ApiTypeMapper(response),
                    TrackId = trackId
                };
            }
            catch
            {
                return null;
            }
        }

        public ReplayDataAndSourceDto GetReplayId(TrackIdAndSourceDto trackData)
        {
            var apiRequest = URLHelper.GetTopReplayUrl(trackData);

            var mapRecordsApiResponse = HttpRequestAsStringSync(apiRequest);
            if (mapRecordsApiResponse == null)
                return null;

            var trackStats = JsonConvert.DeserializeObject<TrackStatsDto>(mapRecordsApiResponse);
            if (trackStats == null)
                return null;

            if (trackStats.Results.Count() == 0)
                return null;

            var topReplay = trackStats.Results.FirstOrDefault();

            try
            {
                return new ReplayDataAndSourceDto()
                {
                    Source = trackData.Source,
                    ReplayId = topReplay.ReplayId.ToString(),
                    Author = topReplay.User.Name,
                    Time = topReplay.ReplayTime.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        public bool DownloadReplay(ReplayDataAndSourceDto replayData)
        {
            var apiRequest = URLHelper.GetDownloadUrl(replayData);

            var replayDataStream = HttpRequestAsStreamSync(apiRequest);
            if (replayDataStream == null)
                return false;

            if (File.Exists("replay.gbx"))
                File.Delete("replay.gbx");

            var newFileStream = File.Create("replay.gbx");
            replayDataStream.CopyTo(newFileStream);

            replayDataStream.Close();
            newFileStream.Close();

            return true;
        }

        public static string HttpRequestAsStringSync(string url)
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

        private TrackIdAndSourceDto SearchByTrackName(string trackName)
        {
            var converterTrackName = Converters.ConvertTrackNameToQuery(trackName);

            foreach (var domain in URLHelper.GetAllUrlDomains())
            {
                var apiRequest = URLHelper.GetSearchByTrackNameUrl(domain, converterTrackName);

                var response = HttpRequestAsStringSync(apiRequest);
                if (response == null)
                    continue;

                var searchDto = JsonConvert.DeserializeObject<SearchDto>(response);

                var searchResults = searchDto.Results;
                if (searchResults.Count() == 0)
                    continue;

                var trackId = searchDto.Results.FirstOrDefault().TrackId.ToString();
                return new TrackIdAndSourceDto()
                {
                    Source = URLHelper.ApiTypeMapper(response),
                    TrackId = trackId
                };

            }

            return null;
        }
    }
}
