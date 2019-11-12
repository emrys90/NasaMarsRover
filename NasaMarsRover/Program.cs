using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NasaMarsRover
{
    class Program
    {
        const string FILENAME = "dates.txt";
        const string API_URL = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos";
        const string API_KEY = "ZbEgN0TodJL1RyyRy5BB1uBOwbDakUoPUqfTPQWt";

        private static readonly string _appPath;

        static Program()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _appPath = Path.Combine(appDataPath, "JesseNorris-NasaMarsRover");

            if (!Directory.Exists(_appPath))
            {
                Directory.CreateDirectory(_appPath);
            }
        }

        static void Main(string[] args)
        {
            if (File.Exists(FILENAME))
            {
                List<DateTime> dates = ParseDates();
                RetrievePhotos(dates).Wait();
            }
            else
            {
                Console.WriteLine(FILENAME + " is missing");
            }

            Console.WriteLine("\nProcessing completed. Press any key to continue.");
            Console.ReadKey();
        }

        private static List<DateTime> ParseDates()
        {
            Console.WriteLine("Parsing dates from " + FILENAME);
            List<DateTime> dates = new List<DateTime>();

            using (StreamReader file = new StreamReader(FILENAME))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    DateTime date;
                    if (DateTime.TryParse(line, out date))
                    {
                        dates.Add(date);
                    }
                    else
                    {
                        Console.WriteLine("Invalid date input: " + line);
                    }
                }
            }

            return dates;
        }

        private static async Task RetrievePhotos(List<DateTime> dates)
        {
            Console.WriteLine("\nFetching image urls from NASA");

            foreach (DateTime date in dates)
            {
                List<string> urls = await GetPhotoUrls(date);
                await DownloadPhotos(date, urls);
            }
        }

        private static async Task<List<string>> GetPhotoUrls(DateTime date)
        {
            string dateFormatted = date.ToString("yyyy-MM-dd");
            Console.Write("Fetching " + dateFormatted + ": ");

            string url = $"{API_URL}?api_key={API_KEY}&earth_date={dateFormatted}";

            List<string> photoUrls = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    PhotosResult results = JsonConvert.DeserializeObject<PhotosResult>(content);
                    foreach (PhotoResult result in results.photos)
                    {
                        photoUrls.Add(result.img_src);
                    }

                    Console.WriteLine(photoUrls.Count + " photo urls retrieved");
                }
                else
                {
                    Console.WriteLine("Failed to retrieve image results");
                }
            }

            return photoUrls;
        }

        private static async Task DownloadPhotos(DateTime date, List<string> photoUrls)
        {
            int downloaded = 0,
                cached = 0,
                errors = 0;

            string dateFormatted = date.ToString("yyyy-MM-dd");
            string path = Path.Combine(_appPath, dateFormatted);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (string url in photoUrls)
            {
                Console.Write($"\r{downloaded} downloaded, {cached} cached, {errors} errors");

                string file = Path.GetFileName(url);
                string filepath = Path.Combine(path, file);
                if (File.Exists(filepath))
                {
                    cached++;
                }
                else
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            var bytes = await client.DownloadDataTaskAsync(url);
                            File.WriteAllBytes(filepath, bytes);
                            downloaded++;
                        }
                    }
                    catch
                    {
                        errors++;
                    }
                }
            }

            Console.WriteLine($"\r{downloaded} downloaded, {cached} cached, {errors} errors\n");
        }
    }
}
