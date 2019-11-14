using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NasaMarsRover
{
    class NasaMarsRoverApi
    {
        public class Result
        {
            public bool Success { get; private set; }
            public string Content { get; private set; }

            public Result(bool success, string content)
            {
                Success = success;
                Content = content;
            }
        }

        private const string API_URL = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos";
        private const string API_KEY = "ZbEgN0TodJL1RyyRy5BB1uBOwbDakUoPUqfTPQWt";

        private const string DATE_FORMAT = "yyyy-MM-dd";

        public string FormatDate(DateTime date)
        {
            string dateFormatted = date.ToString(DATE_FORMAT);
            return dateFormatted;
        }

        public async Task<Result> Fetch(DateTime date)
        {
            string dateFormatted = FormatDate(date);
            string url = $"{API_URL}?api_key={API_KEY}&earth_date={dateFormatted}";

            Result result;
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    result = new Result(true, content);
                }
                else
                {
                    result = new Result(false, null);
                }
            }

            return result;
        }
    }
}
