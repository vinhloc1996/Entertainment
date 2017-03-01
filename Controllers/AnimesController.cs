using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.RegularExpressions;
using Entertainment.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Entertainment.Controllers
{

    [Route("[controller]/[action]/")]
    public class AnimesController : Controller
    {
        // private string address;
        private string movie_id;
        public IList<Episode> Episodes;
        public IList<EpisodeLink> EpisodeLinks;


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string url_encode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string url_decode = System.Uri.UnescapeDataString(url_encode);
                    GetMovieId(url_decode);
                    GetEpisodesId();
                    return RedirectToAction("EpisodePage", "Animes");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpGet("{url_encode}")]
        public IActionResult EpisodePage(string url_encode)
        {
            // TempData["movie_id"] = movie_id;
            string url_decode = System.Uri.UnescapeDataString(url_encode);
            GetMovieId(url_decode);
            // return GetResponseText("http://vuighe.net/api/v2/films/" + movie_id + "/episodes/" + 118735);
            GetEpisodesId();
            // GetEpisodeLink(Episodes[Episodes.Count - 1].id);
            return View(Episodes);
        }

        // [Route("Animes/GetAnimes")]
        [HttpGet("{url_encode}")]
        public string GetAnimes(string url_encode)
        {
            string url_decode = System.Uri.UnescapeDataString(url_encode);
            GetMovieId(url_decode);
            // return GetResponseText("http://vuighe.net/api/v2/films/" + movie_id + "/episodes/" + 118735);
            GetEpisodesId();
            return GetEpisodeLink(Episodes[Episodes.Count - 1].id);
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
        public string GetEpisodesId()
        {
            string addressEpisode = "http://vuighe.net/api/v2/films/" + movie_id + "/episodes?sort=name";
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

        public string GetEpisodeLink(string episode_id)
        {
            string addressEpisodeId = "http://vuighe.net/api/v2/films/" + movie_id + "/episodes/" + episode_id;
            JObject episode_data = JObject.Parse(GetResponseText(addressEpisodeId));
            var ep = episode_data["sources"]["data"];
            IList<JToken> Results = ep.Children().ToList();
            EpisodeLinks = new List<EpisodeLink>();
            foreach (JToken Result in Results)
            {
                EpisodeLink link = JsonConvert.DeserializeObject<EpisodeLink>(Result.ToString());
                link.id = episode_id;
                EpisodeLinks.Add(link);
            }
            return JsonConvert.SerializeObject(EpisodeLinks);
        }

        public string GetResponseText(string url)
        {
            var httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.74 Safari/537.36");
            requestMessage.Headers.Add("Referer", url);
            var response = httpClient.SendAsync(requestMessage).Result;
            var message = response.Content.ReadAsStringAsync().Result;
            return message;
        }

    }
}