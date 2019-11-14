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

        private static NasaMarsRoverApi _api;
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
                _api = new NasaMarsRoverApi();

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
                string dateFormatted = _api.FormatDate(date);
                Console.Write("Fetching " + dateFormatted + ": ");

                NasaMarsRoverApi.Result result = await _api.Fetch(date);
                if (result.Success)
                {
                    List<string> urls = GetPhotoUrls(result.Content);
                    Console.WriteLine(urls.Count + " photo urls retrieved");

                    await DownloadPhotos(date, urls);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve image results");
                }
            }
        }

        private static List<string> GetPhotoUrls(string content)
        {
            List<string> photoUrls = new List<string>();
            PhotosResult results = JsonConvert.DeserializeObject<PhotosResult>(content);
            foreach (PhotoResult result in results.photos)
            {
                photoUrls.Add(result.img_src);
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
