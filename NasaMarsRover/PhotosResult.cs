using System;
using System.Collections.Generic;
using System.Text;

namespace NasaMarsRover
{
    class PhotosResult
    {
        public List<PhotoResult> photos { get; set; }

        public List<string> GetPhotoUrls(string content)
        {
            List<string> photoUrls = new List<string>();
            
            foreach (PhotoResult result in photos)
            {
                photoUrls.Add(result.img_src);
            }

            return photoUrls;
        }
    }
}
