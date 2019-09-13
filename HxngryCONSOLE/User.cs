using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace HxngryCONSOLE
{
    public class User
    {
        public User() { }
        public void InitUser()
        {
            string publicIP = GetPublicIP();
            string jsonData = FetchIPResponse(publicIP);

            FullJSONResponse = jsonData;
            IPData ipData = JsonConvert.DeserializeObject<IPData>(jsonData);
            IP = ipData.IP;
            City = ipData.City;
            State = ipData.Region;
            StateAbbr = ipData.Region_code;
            Postal = ipData.Postal;
            Latitude = ipData.Latitude;
            Longitude = ipData.Longitude;
            Country = ipData.Country_name;
            CountryCode = ipData.Country_code;
        }
        private string GetPublicIP()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }
        private string FetchIPResponse(string publicIP)
        {
            WebClient webClient = new WebClient();
            string jsonData = webClient.DownloadString("https://api.ipdata.co/" + publicIP + "?api-key=" + Environment.GetEnvironmentVariable("IPDATA.CO_KEY"));
            return jsonData;
        }
        private IPData IPData { get; set; }
        public ReconfigureLocation recon { get; set; }
        private string FullJSONResponse { get; set; }
        public string IP { get; set; }
        public string SearchStreetAddress { get; set; }
        public string City { get; set; }
        public string SearchCity { get; set; }
        public string State { get; set; }
        public string StateAbbr { get; set; }
        public string SearchStateAbbr { get; set; }
        public int Postal { get; set; }
        public double Latitude { get; set; }
        public double SearchLatitude { get; set; }
        public double Longitude { get; set; }
        public double SearchLongitude { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string AddressString { get; set; }
        public string Info() => "The user is connected via public IP: " + IP + "\nGeo-Located at: " + City + ", " + State + " " + Postal + ", " + Country + " (" + Latitude + ", " + Longitude + ").\nAll searches will be conducted relative Geo-Location of your IPv4 address.";
    }
}
