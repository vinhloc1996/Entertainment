using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Entertainment.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Entertainment.Controllers
{
    
    [Route("[controller]/[action]")]
    public class AnimesController : Controller
    {
        // private string address;
        private string movie_id;
        public  IList<Episode> Episodes;
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{url_encode}")]
        public string GetAnimes(string url_encode)
        {
            string url_decode = System.Uri.UnescapeDataString(url_encode);
            GetMovieId(url_decode);


            return GetEpisodesId(movie_id);
        }

        /// <summary>Return Movie Id and assign for movie_id at global scope</summary>
        /// <param name="url">Url of current Movie</param>
        /// <return>Movie Id</return>
        public string GetMovieId(string url)
        {
            string source = GetResponseText(url);
            Regex regex = new Regex(@"data-id=""(\d+)""");
            Match match = regex.Match(source);
            movie_id = match.Groups[1].ToString();
            return movie_id;
        }

        /// <summary>Return Episodes Id and assign for episodes_id List at global scope</summary>
        /// <param>None</param>
        /// <return>Json of all Episodes</return>
        public string GetEpisodesId(string id)
        {
            string addressEpisode = "http://vuighe.net/api/v2/films/" + id + "/episodes?sort=name";
            JObject episode_data = JObject.Parse(GetResponseText(addressEpisode));
            var ep = episode_data["data"];
            IList<JToken> Results = ep.Children().ToList();
            Episodes = new List<Episode>();
            foreach (JToken Result in Results)
            {
                Episode episode = JsonConvert.DeserializeObject<Episode>(Result.ToString());
                Episodes.Add(episode);
            }
            return JsonConvert.SerializeObject(Episodes);
        }

        public string GetResponseText(string url)
        {
            var httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
            // requestMessage.Headers.Add("Cookie", "vgv6=eyJpdiI6InNScHN3TGg5VGZqK1pTSWhGTXN5cHc9PSIsInZhbHVlIjoiamlkU2QxMUR3RXZJQVB3Vlo5UjFsejJ2VXdEMlJMeSs5bnY1azEzcmJGQ3JlRHZBSkFjRHN1MFdZblZZQ2VtWHcxUFg1NXRKclwvNk9iSXJhVG5hQWp3PT0iLCJtYWMiOiIwNjM1YjhhZTU3ODhjNzk2Mzg4MTNhYWQxNmI3OTkwYzgxOWY5OGNmMjI5ZDE4YjJhNDk0MDI2MGNhNWE2ODNjIn0%3D; _ga=GA1.2.751952882.1488248639; _gat=1");
            // requestMessage.Headers.Add("Host", "vuighe.net");
            requestMessage.Headers.Add("Referer", url);
            var response = httpClient.SendAsync(requestMessage).Result;
            var message = response.Content.ReadAsStringAsync().Result;
            return message;
        }

    }
}