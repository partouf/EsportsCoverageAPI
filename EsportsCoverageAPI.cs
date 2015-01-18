//-----------------------------------------------------------------------
// <copyright file="EsportsCoverageAPI.cs" company="Public">
//      Contains class CEsportsCoverageAPI
// </copyright>
// <author>Partouf</author>
//-----------------------------------------------------------------------
namespace EsportsCoverage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Diagnostics;

    public class Player
    {
        public int id;
        public int linkcache_id;

        public string playername;
        public string teamname;
        public string twitterlink;
        public string liquipedialink;
        public string twitchlink;
        public string actualname;
        public string aligulaclink;
        public int race_id;
        public string country;
        public string fullname;
        public string realname;
        public string race_imgurl;
        public string race_text;
        public string country_url;
        public string country_long;
    }

    public class StreamDetails
    {
        public string streamname;
        public int event_id;
        public string language;
        public string url;
        public int score1;
        public int score2;
        public Player player1;
        public Player player2;
    }

    /// <summary>Official API class to request resources from website</summary>
    public class EsportsCoverageAPI
    {
        /// <summary>Base url to connect the API to</summary>
        protected string SiteUrl = "http://esportscoverage.net/ajax.php?";

        /// <summary>The user's key to use the API</summary>
        protected string StreamKey = string.Empty;

        /// <summary>Last error that occurred in one of the API's functions</summary>
        public string LastError = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="EsportsCoverageAPI" /> class.
        /// </summary>
        public EsportsCoverageAPI(string streamkey)
        {
            this.StreamKey = streamkey;
        }

        /// <summary>Translates Unix Timestamp to a localized DateTime
        /// source: http://stackoverflow.com/a/250400
        /// </summary>
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Lists the available events to select from that streams may be set to
        /// </summary>
        /// <returns>string array of the event names</returns>
        public Dictionary<int, string> ListEventNames()
        {
            Debug.Assert(SiteUrl != "");

            Dictionary<int, string> events = new Dictionary<int, string>();

            try
            {
                Uri serviceUri = new Uri(SiteUrl + "action=events&func=ListEvents&out=json");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUri);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamreader = new StreamReader(dataStream);

                JObject resultObject = JObject.Parse(streamreader.ReadToEnd());

                JArray resultArr = (JArray)resultObject["result"];
                foreach (var eventObj in resultArr)
                {
                    int startdt = (int)eventObj["startdt"];

                    DateTime dt = UnixTimeStampToDateTime(startdt);
                    string s = dt.ToShortDateString() + " " + dt.ToShortTimeString();

                    events.Add((int)eventObj["id"], (string)eventObj["name"] + " (" + s + ")");
                }

                streamreader.Close();
                dataStream.Close();
            }
            catch (Exception e)
            {
                LastError = e.Message;
                // todo: handle exception properly
            }

            return events;
        }

        /// <summary>
        /// Retreives the current stream details and players registered for this stream, requires key
        /// </summary>
        public StreamDetails GetStreamDetails()
        {
            Debug.Assert(StreamKey != "");

            StreamDetails details = null;
            try
            {
                Uri serviceUri = new Uri(SiteUrl + "action=streamdetails&func=GetCurrentMatchInfoArr&key=" + WebUtility.UrlEncode(StreamKey) + "&nohtml=1");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUri);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamreader = new StreamReader(dataStream);

                JObject resultObject = JObject.Parse(streamreader.ReadToEnd());

                details = JsonConvert.DeserializeObject<StreamDetails>(resultObject["result"].ToString());

                streamreader.Close();
                dataStream.Close();
            }
            catch (Exception e)
            {
                LastError = e.Message;
                // todo: handle exception properly
            }

            return details;
        }

        /// <summary>
        /// Sets the score for this stream, requires key
        /// </summary>
        /// <param name="score1">First player's score</param>
        /// <param name="score2">Second player's score</param>
        public void SetCurrentScore(int score1, int score2)
        {
            Debug.Assert(StreamKey != "");

            try
            {
                Uri serviceUri = new Uri(SiteUrl + "action=streamdetails&func=setPlayerScore&key=" + WebUtility.UrlEncode(StreamKey) + "&score1=" + score1 + "&score2=" + score2 + "&nohtml=1");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUri);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamreader = new StreamReader(dataStream);

                JObject resultObject = JObject.Parse(streamreader.ReadToEnd());

                if (resultObject["result"].ToString() != "OK")
                {
                    // error
                }

                streamreader.Close();
                dataStream.Close();
            }
            catch (Exception e)
            {
                LastError = e.Message;
                // todo: handle exception properly
            }
        }

        /// <summary>
        /// Sets players and score for current stream, requires key. Will try to lookup players on Liquipedia if they weren't previously looked up.
        /// </summary>
        /// <param name="player1">First player's nickname</param>
        /// <param name="player2">Second player</param>
        /// <param name="score1">Optional initial score for player1</param>
        /// <param name="score2">Optional score player2</param>
        /// <returns>true if call succeeded</returns>
        public bool SetCurrentPlayers(string player1, string player2, int score1 = 0, int score2 = 0)
        {
            Debug.Assert(StreamKey != "");

            try
            {
                Uri serviceUri = new Uri(SiteUrl + "action=streamdetails&func=setPlayers&key=" + WebUtility.UrlEncode(StreamKey) + "&player1=" + WebUtility.UrlEncode(player1) + "&player2=" + WebUtility.UrlEncode(player2) + "&score1=" + score1 + "&score2=" + score2 + "&nohtml=1");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUri);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamreader = new StreamReader(dataStream);

                string sResponse = streamreader.ReadToEnd();
                JObject resultObject = JObject.Parse(sResponse);

                if (resultObject["result"].ToString() != "OK")
                {
                    // todo: error
                }

                streamreader.Close();
                dataStream.Close();

                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                // todo: handle exception properly
                return false;
            }
        }

        /// <summary>
        /// Sets Event for current stream, you can get event_id from the ListEventNames() function. Event_id > 0 also sets online flag so the stream will appear on the website until the event is over.
        /// </summary>
        /// <param name="event_id"></param>
        public void SetEvent(int event_id)
        {
            Debug.Assert(StreamKey != "");

            try
            {
                Uri serviceUri = new Uri(SiteUrl + "action=streamdetails&func=setStreamEvent&key=" + WebUtility.UrlEncode(StreamKey) + "&event_id=" + event_id + "&nohtml=1");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUri);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamreader = new StreamReader(dataStream);

                string sResponse = streamreader.ReadToEnd();
                JObject resultObject = JObject.Parse(sResponse);

                if (resultObject["result"].ToString() != "true")
                {
                    // todo: error
                }

                streamreader.Close();
                dataStream.Close();
            }
            catch (Exception e)
            {
                LastError = e.Message;
                // todo: handle exception properly
            }
        }
    }
}
