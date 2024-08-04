using MySpace.Crawler.Web.Models;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;

namespace MySpace.Crawler.Web.Services
{
    public class MySpaceCrawlingService
    {

        public EventHandler<string> Log;

        private HttpClient _httpClient;
        private Regex _baseUrlRegex = new Regex(@"curl \'(?<url>.*)\'.*\\");
        private Regex _headerRegex = new Regex(@"-H \'(?<headerName>.*)\: (?<headerValue>.*)\' \\");
        private Regex _contentRegex = new Regex(@"--data-raw \'page=(?<page>[0-9]{1,}).*&ssid=(?<ssid>.*)\'");

        /// <summary>
        /// This will be written into the header of the webrequest to authorize our request. Idk yet how long this one is valid
        /// </summary>
        private const string _crawlingHashHeaderValue = "MDAwNWQyZTU1Y2UwYmQ0NMOZaiHDrcOeVDLDg3jCsHJzRcKcfTkmwpTDgX9kDMO1w4rDjnPCuAvDrg7CnWLChDwDTMOUEWFLaTNuWwxmwq5vw6LDtjp/TMK+w7NLw4XDgyZKw7wUwqoMRcOlwrrDpMKgw4sfPFvDnBrDrgcqBsO2bsO7YMKdcMKgw60Ew5rDn13CpcOIQ8OneMOJcVbClSjDojpjfVlRB8OSU8KkYw%3D%3D";

        public MySpaceCrawlingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SongItem>> CrawlByTern(string searchTerm, int tries = 10)
        {
            searchTerm = HttpUtility.HtmlEncode(searchTerm);
            var curl = _sampleCurl.Replace("{searchTerm}", searchTerm);
            var res = await Crawl(curl, tries);
            return res;
        }

        public async Task<List<SongItem>> Crawl(string curl, int tries = 10)
        {
            var ret = new List<SongItem>();
            var page = 1;
            while (true)
            {
                var res = await CrawlPage(curl, page, tries);

                if (res?.Count > 0)
                {
                    ret.AddRange(res);

                    //lbSongs.Invoke((MethodInvoker)delegate { lbSongs.Items.AddRange(res.ToArray()); });
                    page = page + 1;
                }
                else
                    break;
            }
            Log?.Invoke(this, $"Finished Crawling. Found {ret.Count} listings");

            return ret;
        }


        private string _sampleCurl = @"curl 'https://myspace.com/ajax/page/search/songs?q={searchTerm}' \
  -H 'accept: */*' \
  -H 'accept-language: de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7' \
  -H 'cache-control: no-cache' \
  -H 'client: persistentId=88092f65-0394-4c31-b351-cb9737026797&screenWidth=1920&screenHeight=1080&timeZoneOffsetHours=-2&visitId=d1f21df2-7407-4797-90ae-ad027bb8c05e&windowWidth=1105&windowHeight=911' \
  -H 'content-type: application/x-www-form-urlencoded; charset=UTF-8' \
  -H 'cookie: playerControlTip=shown=true; persistent_id=pid%3D88092f65-0394-4c31-b351-cb9737026797%26llid%3D-1%26lprid%3D-1%26lltime%3D2024-07-01T13%253A49%253A52.210Z; geo=false; auth_context=pfc=artistSong&action=comment&object=song_71531347; OptanonAlertBoxClosed=2024-07-10T16:56:10.505Z; visit_id=d1f21df2-7407-4797-90ae-ad027bb8c05e; beacons_enabled=true; OptanonConsent=isGpcEnabled=0&datestamp=Thu+Jul+11+2024+00%3A09%3A39+GMT%2B0200+(Mitteleurop%C3%A4ische+Sommerzeit)&version=202406.1.0&isIABGlobal=false&hosts=&landingPath=NotLandingPage&groups=C0003%3A1%2CC0001%3A1%2CC0002%3A1%2CC0004%3A1%2CSSPD_BG%3A1&AwaitingReconsent=false&browserGpcFlag=0&geolocation=DE%3BNW; player=sequenceId=9&paused=true&currentTime=0&volume=0.2170138888888889&mute=false&shuffled=false&repeat=off&mode=queue&pinned=false&at=360&incognito=false&allowSkips=true&ccOn=false' \
  -H 'hash: MWQyZjcxNTBlMjUxOWNmZMKta1vCiyLDgcO6CDgxw7dzwpXCrMOyVcO5F8KTwqMvw4F3w7gmwrTCqSxKw5/CncO5w6lIBsKpw55NawkYd3lswqtPZC/DmMK0UW3Dv8KlMhgnacOsw7PCrVxSYyJMFiBmSsOya8KVw7cwwrLDnsKDG8KyelpeZ8Olw5TCjCjCqcKLwqPDqcKRwrFdwpzDhsKHaMKQwpPDpcOzUF9YVw3CgMOyUVs%3D' \
  -H 'origin: https://myspace.com' \
  -H 'pragma: no-cache' \
  -H 'priority: u=1, i' \
  -H 'referer: https://myspace.com/search/songs?q={searchTerm}' \
  -H 'sec-ch-ua: ""Not/A)Brand"";v=""8"", ""Chromium"";v=""126"", ""Google Chrome"";v=""126""' \
  -H 'sec-ch-ua-mobile: ?0' \
  -H 'sec-ch-ua-platform: ""Windows""' \
  -H 'sec-fetch-dest: empty' \
  -H 'sec-fetch-mode: cors' \
  -H 'sec-fetch-site: same-origin' \
  -H 'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36' \
  -H 'x-requested-with: XMLHttpRequest' \
  --data-raw 'page=2&ssid=316c7bb5-b505-419e-80e2-10e018bfa712'";

        private async Task<List<SongItem>> CrawlPage(string text, int page = 1, int tries = 10)
        {

            while (tries > 0)
            {
                try
                {

                    Log?.Invoke(this, $"Crawling page '{page}' | Tries left: {tries}");
                    var ret = new List<SongItem>();
                    var responseMessage = await ParseCurl(text, page);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var tmp = await responseMessage.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(tmp) && tmp != "{}")
                        {
                            var doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(tmp);

                            foreach (var listNode in doc.DocumentNode.SelectNodes("//li/div[@class='flex']"))
                            {
                                var titleNode = listNode.SelectSingleNode("./div[@class='title']");
                                var artistNode = listNode.SelectSingleNode("./div[@class='artist']");
                                var albumNode = listNode.SelectSingleNode("./div[@class='album']");
                                var dateNode = listNode.SelectSingleNode("./div[@class='date']");
                                var durationNode = listNode.SelectSingleNode("./div[@class='duration']");


                                ret.Add(new SongItem
                                {
                                    Album = albumNode?.InnerText.Trim(),
                                    Artist = artistNode?.InnerText.Trim(),
                                    Date = dateNode?.InnerText.Trim(),
                                    Duration = durationNode?.InnerText.Trim(),
                                    Url = "https://myspace.com" + titleNode?.SelectSingleNode("./a")?.Attributes["href"]?.Value,
                                    Title = titleNode?.InnerText.Trim()
                                });

                            }
                            return ret;
                        }
                        else
                        {
                            tries = tries - 1;
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log?.Invoke(this, $"Error: {ex}");
                }
            }
            return null;
        }

        private async Task<HttpResponseMessage> ParseCurl(string curl, int page = 1)
        {
            var baseUrl = _baseUrlRegex.Match(curl).Groups["url"].Value;

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            request.Method = HttpMethod.Post;
            
            foreach (Match headeRegexResult in _headerRegex.Matches(curl))
            {
                if (headeRegexResult.Success)
                {
                    var name = headeRegexResult.Groups["headerName"].Value;
                    var value = headeRegexResult.Groups["headerValue"].Value;

                    switch (name)
                    {
                        //case "accept":
                        //    //httpClient.DefaultRequestHeaders.Accept = new System.Net.Http.Headers.HttpHeaderValueCollection<System.Net.Http.Headers.MediaTypeWithQualityHeaderValue>() headeRegexResult.Groups["headerValue"].Value;
                        //    break;
                        case "content-type":
                            //httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(value));
                        break;
                        //case "referer":
                        //    webRequest.Referer = headeRegexResult.Groups["headerValue"].Value;
                        //    break;
                        //case "user-agent":
                        //    webRequest.UserAgent = headeRegexResult.Groups["headerValue"].Value;
                        //    break;
                        //case "hash":
                        //    webRequest.Headers.Add(headeRegexResult.Groups["headerName"].Value, headeRegexResult.Groups["headerValue"].Value);
                        //    break;
                        case "Access-Control-Allow-Origin":
                            break;
                        default:
                            request.Headers.Add(name, value);
                            break;
                    }

                }
            }
            
            var ssid = _contentRegex.Match(curl).Groups["ssid"].Value;
            var stringContent = new StringContent($"page={page}&ssid={ssid}");
            request.Content = stringContent;
            //using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
            //{
            //    requestWriter.Write($"page={page}&ssid={ssid}");
            //}

            return await _httpClient.SendAsync(request);
        }
    }
}
