using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace Hotsoup
{
    class Trakt
    {
        const string API = "1133a846989d00fd37b9c6326b0ed2e9";
        public static UserCredentials userCred;
        public static bool traktEnabled;
        public static bool trendEnabled;
        public static int timeout = 5000;
        public static int trendingCount = 10;

        public bool GetMovies()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.trakt.tv/movies/trending.json/" + API);
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeout;

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Movie.List = JsonConvert.DeserializeObject<List<Movie>>(result).Take(trendingCount).ToList();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetShows()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.trakt.tv/shows/trending.json/" + API);
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeout;

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Serie.List = JsonConvert.DeserializeObject<List<Serie>>(result).Take(trendingCount).ToList();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetAirs()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.trakt.tv/user/calendar/shows.json/" + API + "/" + userCred.username);
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeout;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(userCred, Formatting.Indented);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        tCalender.Airs = JsonConvert.DeserializeObject<List<tCalender>>(result).Take(10).ToList();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetProgress()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.trakt.tv/user/progress/watched.json/" + API + "/" + userCred.username + "/all/recently-aired");
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeout;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(userCred, Formatting.Indented);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        tProgress.Prog = JsonConvert.DeserializeObject<List<tProgress>>(result).ToList();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

    class Serie
    {
        public string title { get; set; }

        public static List<Serie> List = new List<Serie>();
        
    }

    class Movie
    {
        public string title { get; set; }

        public static List<Movie> List = new List<Movie>();
    }

    class tCalender
    {
        public string date { get; set; }
        public List<Episode> episodes { get; set; }

        public static List<tCalender> Airs = new List<tCalender>();
    }

    class tProgress
    {
        public JObject show { get; set; }
        public JObject progress { get; set; }

        public static List<tProgress> Prog = new List<tProgress>();
    }

    class Episode
    {
        public JObject show { get; set; }
    }

    public class UserCredentials
    {
        public UserCredentials(string user, string pass)
        {
            username = user;
            password = pass;
        }

        public string username;
        public string password;
    }

}
