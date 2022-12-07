using LogicStorage.Dtos.ApiRequests.TMXSearch;
using LogicStorage.Dtos.ApiRequests.TMXTrackData;
using LogicStorage.Dtos.ReplayList;
using LogicStorage.Dtos.Track;
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
        private static HttpClient HttpClient { get; set; }

        public RequestHandler()
        {
            HttpClient = new HttpClient();
        }

        public TrackAndSourceDto GetTrackIdAndSource(TrackDataDto trackData)
        {
            var uidCheck = SearchByUID(trackData.UID);
            if (uidCheck != null)
                return uidCheck;

            var nameCheck = SearchByTrackName(trackData.AuthorName);
            if (nameCheck != null)
                return nameCheck;

            return null;
        }

        public TrackAndSourceDto SearchByUID(string uid)
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

                return new TrackAndSourceDto() 
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

        public List<APITrackDataResultDto> GetReplayList(TrackAndSourceDto trackData)
        {
            var apiRequest = URLHelper.GetTopReplayUrl(trackData);

            var mapRecordsApiResponse = HttpRequestAsStringSync(apiRequest);
            if (mapRecordsApiResponse == null)
                return null;

            var trackStats = JsonConvert.DeserializeObject<APITrackDataDto>(mapRecordsApiResponse);
            if (trackStats == null)
                return null;

            if (trackStats.Results.Count() == 0)
                return null;

            return trackStats.Results;
        }

        public bool DownloadReplay(ReplayDto replayData)
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
            var response = HttpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                return null;
        }

        public Stream HttpRequestAsStreamSync(string url)
        {
            var response = HttpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStreamAsync().Result;
            else
                return null;
        }

        private TrackAndSourceDto SearchByTrackName(string trackName)
        {
            var converterTrackName = Converters.ConvertTrackNameToQuery(trackName);

            foreach (var domain in URLHelper.GetAllUrlDomains())
            {
                var apiRequest = URLHelper.GetSearchByTrackNameUrl(domain, converterTrackName);

                var response = HttpRequestAsStringSync(apiRequest);
                if (response == null)
                    continue;

                var searchDto = JsonConvert.DeserializeObject<APISearchDto>(response);

                var searchResults = searchDto.Results;
                if (searchResults.Count() == 0)
                    continue;

                var trackId = searchDto.Results.FirstOrDefault().TrackId.ToString();
                return new TrackAndSourceDto()
                {
                    Source = URLHelper.ApiTypeMapper(response),
                    TrackId = trackId
                };
            }

            return null;
        }
    }
}
