using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace HxngryCONSOLE
{
    public class SelectedPlace
    {
        public List<object> html_attributions { get; set; }
        public Result result { get; set; }
        public string status { get; set; }
        public string InitPlaceInfo(List<PlacesData.Result> result, int index)
        {
            WebClient webClient = new WebClient();
            string htmlRequest = "https://maps.googleapis.com/maps/api/place/details/json?placeid=" + result[index].place_id + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            string jsonData = webClient.DownloadString(htmlRequest);

            return jsonData;
        }
        public string[] Info()
        {
            string header = "##########################################################################################################";
            int eachSide = (header.Length - this.result.name.Length) / 2;
            if (eachSide % 2 != 0)
            {
                eachSide++;
            }
            string side = "";
            for (int i = 0; i < eachSide - 1; ++i)
            {
                side += "#";
            }
            string printString = header + "\n" + side + " " + this.result.name + " " + side + "\n" + header + "\n\n";
            string priceLevel = "";
            if (this.result.price_level == 0)
            {
                priceLevel += "N/A";
            }
            else
            {
                for (int i = 0; i < Convert.ToInt32(this.result.price_level); ++i)
                {
                    priceLevel += "$";
                }
            }
            printString += "Rating: " + this.result.rating + " (" + this.result.user_ratings_total + " Total Ratings) -- Price Level: " + priceLevel;
            printString += "\nGeo-Located at: " + this.result.vicinity + " (" + this.result.geometry.location.lat + ", " + this.result.geometry.location.lng + ")";
            printString += "\nPhone Contact: " + this.result.formatted_phone_number + " (" + this.result.international_phone_number + ")\nWebsite: " + this.result.website;
            printString += "\n\nWeekly Hours:";
            foreach (string str in this.result.opening_hours.weekday_text)
            {
                printString += "\n" + str;
            }
            printString += "\n\nRestaurant Tags: ";
            string tags = "";
            foreach (string str in this.result.types)
            {
                tags += str.Replace("_", " ") + ", ";
            }
            tags = tags.Substring(0, tags.Length - 2) + ".";
            printString += tags;
            string reviewString = "";
            reviewString += "\n\n---------------- Latest Reviews Listed Below ----------------";
            foreach (Review review in this.result.reviews)
            {
                reviewString += "\n\nRating: " + review.rating + " -- Author: " + review.author_name + " (" + review.relative_time_description + ")";
                reviewString += "\nWritten Review: " + review.text;
            }
            string[] returnArray = new string[] { printString, reviewString };
            return returnArray;

        }
        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Viewport
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
            public Viewport viewport { get; set; }
        }

        public class Close
        {
            public int day { get; set; }
            public string time { get; set; }
        }

        public class Open
        {
            public int day { get; set; }
            public string time { get; set; }
        }

        public class Period
        {
            public Close close { get; set; }
            public Open open { get; set; }
        }

        public class OpeningHours
        {
            public bool open_now { get; set; }
            public List<Period> periods { get; set; }
            public List<string> weekday_text { get; set; }
        }

        public class Photo
        {
            public int height { get; set; }
            public List<string> html_attributions { get; set; }
            public string photo_reference { get; set; }
            public int width { get; set; }
        }

        public class PlusCode
        {
            public string compound_code { get; set; }
            public string global_code { get; set; }
        }

        public class Review
        {
            public string author_name { get; set; }
            public string author_url { get; set; }
            public string language { get; set; }
            public string profile_photo_url { get; set; }
            public int rating { get; set; }
            public string relative_time_description { get; set; }
            public string text { get; set; }
            public int time { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string adr_address { get; set; }
            public string formatted_address { get; set; }
            public string formatted_phone_number { get; set; }
            public Geometry geometry { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public string international_phone_number { get; set; }
            public string name { get; set; }
            public OpeningHours opening_hours { get; set; }
            public List<Photo> photos { get; set; }
            public string place_id { get; set; }
            public PlusCode plus_code { get; set; }
            public int price_level { get; set; }
            public double rating { get; set; }
            public string reference { get; set; }
            public List<Review> reviews { get; set; }
            public string scope { get; set; }
            public List<string> types { get; set; }
            public string url { get; set; }
            public int user_ratings_total { get; set; }
            public int utc_offset { get; set; }
            public string vicinity { get; set; }
            public string website { get; set; }
        }
    }
}
