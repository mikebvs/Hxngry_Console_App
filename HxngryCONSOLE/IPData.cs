using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace HxngryCONSOLE
{
    public class IPData
    {
        public string IP { get; set; }
        public bool EU { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Region_code { get; set; }
        public string Country_name { get; set; }
        public string Country_code { get; set; }
        public string Continent_name { get; set; }
        public string Continent_code { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Postal { get; set; }
        public int Calling_code { get; set; }
        public string Flag { get; set; }
        public string Emoji_flag { get; set; }
        public string Emoji_unicode { get; set; }
        public asnClass Asn { get; set; }
        public langArr[] Languages { get; set; }
        public currency Currency { get; set; }
        public time_zone Time_zone { get; set; }
        public threat Threat { get; set; }
        public int Count { get; set; }
    }
    public class asnClass
    {
        public string Asn { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Route { get; set; }
        public string Type { get; set; }
    
    }
    public class lang
    {
        public string Name { get; set; }
        public string Native { get; set; }
    }
    public class langArr
    {
        public List<lang> Lang { get; set; }
    }
    public class currency
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public char Symbol { get; set; }
        public char Native { get; set; }
        public string Plural { get; set; }
    }
    public class time_zone
    {
        public string Name { get; set; }
        public string Abbr { get; set; }
        public string Offset { get; set; }
        public bool Is_dst { get; set; }
        public string Current_time { get; set; }
    }
    public class threat
    {
        public bool Is_tor { get; set; }
        public bool Is_proxy { get; set; }
        public bool Is_anonymous { get; set; }
        public bool Is_known_attacker { get; set; }
        public bool Is_known_abuser { get; set; }
        public bool Is_threat { get; set; }
        public bool Is_bogon { get; set; }
    }
}
