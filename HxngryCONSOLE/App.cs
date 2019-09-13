using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using System.Threading;
using System.Linq;

namespace HxngryCONSOLE
{
    public class App : User
    {
        public App() { }
        public User user { get; set; }
        public double SearchDistance { get; set; }
        public string SearchKeyWord { get; set; }
        public List<string> PlaceTypes { get; set; }
        public void InitApp(User user)
        {
            this.user = user;
            InitLocation();
            InitTwilio();
            InitPhoneContact();
            InitPlacesTypes();
        }
        public void MainLoop()
        {
            while (true)
            {
                PrintBanner();
                Console.WriteLine("Please select one of the following options to begin...");
                Console.WriteLine("\n[F] Food Search\n[G] Grocery Search\n[T] Type Search\n[N] Search by Name\n[Q] To Quit");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if (userChoice.KeyChar == 'f' || userChoice.KeyChar == 'F')
                {
                    BuildFoodRequest();
                }
                else if (userChoice.KeyChar == 'g' || userChoice.KeyChar == 'G')
                {
                    //GrocerySearch();
                }
                else if (userChoice.KeyChar == 't' || userChoice.KeyChar == 'T')
                {
                    BuildTypeRequest();
                }
                else if(userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
                {
                    BuildNameRequest();
                }
                else if(userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                {
                    Environment.Exit(0);
                }

            }
        }
        public void InitTwilio()
        {
            TwilioClient.Init(
                Environment.GetEnvironmentVariable("TWILIO_KEY"),
                Environment.GetEnvironmentVariable("TWILIO_AUTH"),
                Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
            );
        }
        public void InitLocation()
        {
            PrintBanner();
            Console.WriteLine("Currently Hxngry will be conducting searches based on the location of your Public IPv4 address.\n");
            Console.WriteLine("IPv4 Address Location: " + this.user.City + ", " + this.user.State + " " + this.user.Postal + ", " + this.user.Country + ". (" + this.user.Latitude + ", " + this.user.Longitude + ")\n");
            while (true)
            {
                Console.WriteLine("[Y] Change your current address\n[N] Continue with the current address\n[Q] Quit Hxngry");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if(userChoice.KeyChar == 'y' || userChoice.KeyChar == 'Y')
                {
                    ChangeSearchParams();
                    PrintBanner();
                    Console.WriteLine("Your new address is the following: " + this.user.AddressString);
                    Console.WriteLine("\nIs this correct?\n[Y] Continue\n[N] Change your address entry\n[Q] Quit Hxngry");
                    userChoice = Console.ReadKey();
                    if(userChoice.KeyChar == 'y' || userChoice.KeyChar == 'Y')
                    {
                        break;
                    }
                    else if (userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
                    {
                        ChangeSearchParams();
                    }
                    else if(userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                    {
                        PrintBanner();
                        Environment.Exit(0);
                    }
                }
                else if(userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                {
                    PrintBanner();
                    Environment.Exit(0);
                }
                else
                {
                    this.user.SearchLatitude = this.user.Latitude;
                    this.user.SearchLongitude = this.user.Longitude;
                    this.user.SearchCity = this.user.City;
                    this.user.SearchStateAbbr = this.user.StateAbbr;
                    break;
                }
            }
        }
        public void InitPlacesTypes()
        {
            string places = "accounting;airport;amusement_park;aquarium;art_gallery;atm;bakery;bank;bar;beauty_salon;bicycle_store;book_store;bowling_alley;bus_station;cafe;campground;car_dealer;car_rental;car_repair";
            places += ";car_wash;casino;cemetery;church;city_hall;clothing_store;convenience_store;courthouse;dentist;department_store;doctor;electrician;electronics_store;embassy;fire_station;florist;funeral_home";
            places += ";furniture_store;gas_station;gym;hair_care;hardware_store;hindu_temple;home_goods_store;hospital;insurance_agency;jewelry_store;laundry;lawyer;library;liquor_store;local_government_office;locksmith";
            places += ";lodging;meal_delivery;meal_takeaway;mosque;movie_rental;movie_theater;moving_company;museum;night_club;painter;park;parking;pet_store;pharmacy;physiotherapist;plumber;police;post_office";
            places += ";real_estate_agency;restaurant;roofing_contractor;rv_park;school;shoe_store;shopping_mall;spa;stadium;storage;store;subway_station;supermarket;synagogue;taxi_stand;train_station;transit_station";
            places += ";travel_agency;veterinary_care;zoo";
            char[] separator = { ';' };
            this.PlaceTypes = places.Split(separator).ToList();
        }
        public void ChangeSearchParams()
        {
            PrintBanner();
            Console.WriteLine("You have selected to change your search address...");
            Console.WriteLine("Please follow the instructions and provide the necessary information to determine the search location. Press any key to continue...");
            Console.ReadKey();
            PrintBanner();
            Console.WriteLine("We will require various parts of the address you'd like to search from. The format will be as such:");
            Console.WriteLine("Address Formatting Example: 2266 Candlyland Drive, CandyCity, CS --> <STREET ADDRESS>, <CITY>, <STATE ABBREVIATION>");
            Console.WriteLine("\nPlease enter your street address.\n");
            string addr = Console.ReadLine();
            this.user.SearchStreetAddress = addr;
            PrintBanner();
            Console.WriteLine("Address Formatting Example: 2266 Candlyland Drive, CandyCity, CS --> <STREET ADDRESS>, <CITY>, <STATE ABBREVIATION>");
            Console.WriteLine("\nPlease enter your city name.\n");
            addr = Console.ReadLine();
            this.user.SearchCity = addr;
            PrintBanner();
            Console.WriteLine("Address Formatting Example: 2266 Candlyland Drive, CandyCity, CS --> <STREET ADDRESS>, <CITY>, <STATE ABBREVIATION>");
            Console.WriteLine("\nPlease enter your state abbreviation.\n");
            addr = Console.ReadLine();
            this.user.SearchStateAbbr = addr.ToUpper();
            this.user.AddressString = this.user.SearchStreetAddress + ", " + this.user.SearchCity + ", " + this.user.SearchStateAbbr;
            FetchLatLong(this.user.AddressString);
        }
        private void FetchLatLong(string address)
        {
            address = address.Replace(" ", "+");
            string latlong = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(latlong);
            ReconfigureLocation.GeocodingReconfigure.Location updatedInfo = JsonConvert.DeserializeObject<ReconfigureLocation.GeocodingReconfigure.Location>(json);
            this.user.SearchLatitude = updatedInfo.lat;
            this.user.SearchLongitude = updatedInfo.lng;

        }
        public void InitPhoneContact()
        {
            while (true)
            {
                bool completedPhoneInit = false;
                PrintBanner();
                var count = 0;
                var attempts = 0;
                while (completedPhoneInit == false)
                {
                    PrintBanner();
                    if(attempts == 0)
                    {
                        Console.WriteLine("Please enter your Phone Number to be contacted via the Hxngry Service in the following format: +1###-###-####\n");
                    }
                    else
                    {
                        Console.WriteLine("The previous phone number entry was incorrect. Please check the formatting of the entry.");
                        Console.WriteLine("The format can be either of the following: \"+1###-###-####\" OR \"###-###-####\".\n");
                    }
                    var phone = Console.ReadLine();
                    completedPhoneInit = this.SetPhoneContact(phone);
                    try
                    {
                        var outgoingCallerIds = OutgoingCallerIdResource.Read(phoneNumber: new Twilio.Types.PhoneNumber(this.user.Phone));
                        count = 0;
                        foreach(var record in outgoingCallerIds)
                        {
                            count++;
                        }
                        Console.WriteLine("COUNT: " + count);
                    }
                    catch(Exception e)
                    {
                        attempts++;
                        continue;
                    }
                    attempts++;
                    break;
                }
                Console.WriteLine("COUNT: " + count);
                PrintBanner();
                if (count == 0)
                {
                    Console.WriteLine("We must verify your number with Hxngry in order to send SMS messages.\nPlease follow the instructions provided on screen.\n\n Press any key to continue...");
                    Console.ReadKey();
                    VerifyCallerID();
                }
                else
                {
                    Console.WriteLine("Your number (" + this.user.Phone + ") is already verified with us, thank you for returning to Hxngry.");
                }
                Console.WriteLine("\n[Space Bar] To continue\n[R] To Resubmit Phone Number\n[Q] To Quit");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if(userChoice.KeyChar == ' ')
                {
                    break;
                }
                else if(userChoice.KeyChar == 'r' || userChoice.KeyChar == 'R')
                {
                    continue;
                }
                else if(userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                {
                    Environment.Exit(0);
                }
            }
        }
        public void VerifyCallerID()
        {
            PrintBanner();
            Console.WriteLine("Please enter the name of the number that Hxngry will be using. (Mike's Phone, John's iPhone, Mary, etc)");
            var numName = Console.ReadLine();
            var validationRequest = ValidationRequestResource.Create(
                friendlyName: numName,
                phoneNumber: new Twilio.Types.PhoneNumber(this.user.Phone)
            );
            Console.WriteLine("You will receive a call within the next minute, the call will request a verification code.\n");
            Console.WriteLine("This is your verification code: " + validationRequest.ValidationCode);
            Console.WriteLine("Once you have entered the verification code you may press any key to continue...");
            AppLoading();
            PrintBanner();
            Console.WriteLine("You will now be sent an SMS message verifying your number's authentication.");
            var message = MessageResource.Create(
                body: "Number verified.",
                from: new Twilio.Types.PhoneNumber("+15204629326"),
                to: new Twilio.Types.PhoneNumber(this.user.Phone)
            );
        }
        public bool SetPhoneContact(string phone)
        {
            bool acceptedInput = false;
            phone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace(" ", "");
            if(phone.Length == 10)
            {
                phone = phone.Insert(0, "+1");
            }
            if(phone.Length == 12)
            {
                acceptedInput = true;
            }
            else
            {
                return false;
            }
            this.user.Phone = phone;
            return acceptedInput;
        }
        //TYPE SEARCH
        private void BuildTypeRequest()
        {
            PrintBanner();
            double searchRadius = SearchRadius();
            string searchKey = SearchKey();
            string type = TypeKey().ToLower();
            string typeRequest = "";
            if (searchKey == "")
            {
                typeRequest = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + this.user.Latitude + "," + this.user.Longitude + "&radius=" + searchRadius + "&type=" + type + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            }
            else
            {
                typeRequest = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + this.user.Latitude + "," + this.user.Longitude + "&radius=" + searchRadius + "&type=" + type + "&keyword=" + searchKey + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            }

            List<PlacesData.Result> results = PlacesDataRequest(typeRequest);
            SortByRating(results);
            PrintData(results);
            ChoosePlace(results);
        }
        private string TypeKey()
        {
            while (true)
            {
                PrintBanner();
                Console.WriteLine("Would you like to see a list of venue types?");
                Console.WriteLine("\n[H] List of Applicable Venue Types\n[N] Continue to Venue Type Input\n[R] Return to Search Menu\n[Q] Quit\n");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if(userChoice.KeyChar == 'h' || userChoice.KeyChar == 'H')
                {
                    ListTypes();
                }
                else if(userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
                {
                    PrintBanner();
                    Console.WriteLine("Please enter the type of venue you would like to search for...\n\n");
                    string key = Console.ReadLine();
                    return key;
                }
                else if (userChoice.KeyChar == 'r' || userChoice.KeyChar == 'R')
                {
                    MainLoop();
                }
                else if (userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                {
                    Environment.Exit(0);
                }
                else
                {
                    continue;
                }
            }
        }
        private void ListTypes()
        {
            PrintBanner();
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            for (int i = 0; i < this.PlaceTypes.Count; ++i)
            {
                if(i + 5 < this.PlaceTypes.Count)
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i],this.PlaceTypes[i+1],this.PlaceTypes[i+2],this.PlaceTypes[i+3],this.PlaceTypes[i+4],this.PlaceTypes[i+5]);
                    i += 5;
                }
                else if(i + 4 < this.PlaceTypes.Count)
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i], this.PlaceTypes[i + 1], this.PlaceTypes[i + 2], this.PlaceTypes[i + 3], this.PlaceTypes[i + 4], "-----");
                    i += 4;
                }
                else if (i + 3 < this.PlaceTypes.Count)
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i], this.PlaceTypes[i + 1], this.PlaceTypes[i + 2], this.PlaceTypes[i + 3], "-----", "-----");
                    i += 3;
                }
                else if (i + 2 < this.PlaceTypes.Count)
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i], this.PlaceTypes[i + 1], this.PlaceTypes[i + 2], "-----", "-----", "-----");
                    i += 2;
                }
                else if (i + 1 < this.PlaceTypes.Count)
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i], this.PlaceTypes[i + 1], "-----", "-----", "-----", "-----");
                    i += 1;
                }
                else
                {
                    Console.WriteLine("| {0,-25} | {1,-25} | {2,-25} | {3,-25} | {4,-25} | {5,-25} |", this.PlaceTypes[i], "-----", "-----", "-----", "-----", "-----");
                }
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("\nPress Any Key to Continue...");
            Console.ReadKey();
        }
        //PLACES SEARCH METHODS
        private void BuildNameRequest()
        {
            PrintBanner();
            double searchRadius = SearchRadius();
            string searchKey = SearchKey();
            string nameRequest = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + this.user.Latitude + "," + this.user.Longitude + "&radius=" + searchRadius + "&keyword=" + searchKey + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            List<PlacesData.Result> results = PlacesDataRequest(nameRequest);
            SortByRating(results);
            PrintData(results);
            ChoosePlace(results);


        }
        private List<PlacesData.Result> SortByRating(List<PlacesData.Result> results)
        {
            results.Sort((y, x) => x.rating.CompareTo(y.rating));
            return results;
        }
        private void PrintData(List<PlacesData.Result> results)
        {
            Console.WriteLine("Search Complete. The results will be listed from highest to lowest rating below...\n");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("| {0,-45} | {1,-28} | {2,-14} | {3}", "Restaurant Name", "Ratings", "Distance", "Address");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
            for (int i = 0; i < results.Count; ++i)
            {
                string reports = "(" + results[i].rating + " via " + results[i].user_ratings_total + " user reports)";
                int price = results[i].price_level;
                string pricing = "";
                if(price == 0)
                {
                    pricing = "N/A";
                }
                else
                {
                    for (int j = 0; j < price; ++j)
                    {
                        pricing = pricing + "$";
                    }
                }
                string name = "";
                if (results[i].name.Length > 35)
                {
                    name = results[i].name.Substring(0,33) + "..." + " (" + pricing + ")";
                }
                else
                {
                    name = results[i].name + " (" + pricing + ")";
                }
                Console.WriteLine("| {0,-45} | {1,-28} | {2,-14} | {3}", name, reports, Math.Round(Calculations.distance(this.user.Latitude, this.user.Longitude, results[i].geometry.location.lat, results[i].geometry.location.lng, 'M'), 2).ToString() + " mi.", results[i].vicinity);
            }
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }
        //FOOD SEARCH METHODS
        private void BuildFoodRequest()
        {
            PrintBanner();
            double searchRadius = SearchRadius();
            string searchKey = SearchKey();
            string foodRequest = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + this.user.Latitude + "," + this.user.Longitude + "&radius=" + searchRadius + "&type=restaurant&keyword=" + searchKey + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            List<PlacesData.Result> results = PlacesDataRequest(foodRequest);
            
            results = CleanData(searchKey, searchRadius, results);
            PrintNearbyFoodData(results);
            ChoosePlace(results);
        }
        private void ChoosePlace(List<PlacesData.Result> results)
        {
            Console.WriteLine("\n\nSearch Successful...");
            Console.WriteLine("\n[N] Select Specific Venue\n[R] Conduct Another Search\n[Q] Quit");
            ConsoleKeyInfo userChoice = Console.ReadKey();
            if (userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
            {
                Console.WriteLine("Please enter the name of the place you'd like more information on.\n");
                string place = Console.ReadLine();
                if (results.Exists(a => a.name == place))
                {
                    PrintBanner();
                    SelectedPlace placeInfo = new SelectedPlace();
                    int index = results.FindIndex(a => a.name == place);
                    string specificJson = placeInfo.InitPlaceInfo(results, index);
                    placeInfo = JsonConvert.DeserializeObject<SelectedPlace>(specificJson);
                    string[] printData = placeInfo.Info();
                    Console.WriteLine(printData[0]);
                    bool userReady = false;
                    while (userReady == false)
                    {
                        Console.WriteLine("\n[Space Bar] Display The Most Recent Reviews\n[Enter] Select This Restaurant and Notify via SMS\n[B] Return to Main Menu");
                        userChoice = Console.ReadKey();
                        if(userChoice.KeyChar == 'b' || userChoice.KeyChar == 'B')
                        {
                            PrintBanner();
                            break;
                        }
                        else if (userChoice.KeyChar == ' ')
                        {
                            Console.WriteLine("\n\n" + printData[1] + "\n\n[Space Bar] Return to Main Menu.\n[Enter] Select This Restaurant and Notify via SMS.");
                            userChoice = Console.ReadKey();
                            if(userChoice.KeyChar == (char)ConsoleKey.Enter)
                            {
                                SelectPlace(placeInfo);
                            }
                            else if(userChoice.KeyChar == ' ')
                            {
                                PrintBanner();
                                break;
                            }
                            Console.ReadKey();
                            PrintBanner();
                            break;
                        }
                        else if(userChoice.KeyChar == (char)ConsoleKey.Enter)
                        {
                            SelectPlace(placeInfo);
                        }
                    }
                }
            }
            else if (userChoice.KeyChar == 'r' || userChoice.KeyChar == 'R')
            {
                
            }
            else if (userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
            {
                PrintBanner();
                Environment.Exit(0);
            }

        }
        private void SelectPlace(SelectedPlace placeInfo)
        {
            PrintBanner();
            Console.WriteLine("You have selected " + placeInfo.result.name + ".");
            Console.WriteLine("Would you like to add a personal message to the notification?\n\n[Y] Yes\n[N] No");
            string customMessage = "";
            while (true)
            {
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if(userChoice.KeyChar == 'y' || userChoice.KeyChar == 'Y')
                {
                    PrintBanner();
                    Console.WriteLine("Venue: " + placeInfo.result.name + "\nLocal Address: " + placeInfo.result.vicinity);
                    Console.WriteLine("\nPlease enter the message you would like to send along with the notification.\n");
                    customMessage = Console.ReadLine();
                    break;
                }
                else if(userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
                {
                    break;
                }
            }
            Console.WriteLine("Applicable SMS numbers will be notified shortly...");
            string messageBody = "\nUser Message: " + customMessage + "\n\nThe seleceted venue is: " + placeInfo.result.name + " located at " + placeInfo.result.address_components[0].long_name + " " + placeInfo.result.address_components[1].long_name + ", " + placeInfo.result.address_components[3].long_name + ", " + placeInfo.result.address_components[4].short_name + " " + placeInfo.result.address_components[6].long_name;
            var message = MessageResource.Create(
                body: messageBody,
                from: new Twilio.Types.PhoneNumber("+15204629326"),
                to: new Twilio.Types.PhoneNumber("+18043705689")
            );
            AppLoading();
            PrintBanner();
            Console.WriteLine("Message sent.");
            Console.WriteLine("Closing Application");
            Console.Write(".");
            Thread.Sleep(100);
            Console.Write(".");
            Thread.Sleep(100);
            Console.Write(".");
            Thread.Sleep(100);
            Console.Write(".");
            Thread.Sleep(100);
            Console.Write(".");
            Thread.Sleep(100);
            Environment.Exit(0);
        }
        private void PrintNearbyFoodData(List<PlacesData.Result> results)
        {
            Console.WriteLine("Search Complete. The results will be listed from highest to lowest rating below...\n");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("| {0,-45} | {1,-28} | {2,-14} | {3}", "Restaurant Name", "Ratings", "Distance", "Address");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
            for (int i = 0; i < results.Count; ++i)
            {
                string reports = "(" + results[i].rating + " via " + results[i].user_ratings_total + " user reports)";
                int price = results[i].price_level;
                string pricing = "";
                for (int j = 0; j < price; ++j)
                {
                    pricing = pricing + "$";
                }
                string name = results[i].name + " (" + pricing + ")";
                Console.WriteLine("| {0,-45} | {1,-28} | {2,-14} | {3}", name, reports, Math.Round(Calculations.distance(this.user.Latitude, this.user.Longitude, results[i].geometry.location.lat, results[i].geometry.location.lng, 'M'), 2).ToString() + " mi.", results[i].vicinity);
            }
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }
        private List<PlacesData.Result> CleanData(string searchItem, double numMiles, List<PlacesData.Result> results)
        {
            PrintBanner();
            Console.WriteLine("Would you like to only include venues that offer takeout? (y/n)");
            var searchString = "Searching for " + searchItem + " within " + Math.Round(Convert.ToDouble(numMiles/1609.344),2).ToString() + " miles with the following parameters:";
            var takeout = Console.ReadLine().ToLower();
            while (takeout != "y" && takeout != "yes" && takeout != "n" && takeout != "no")
            {
                PrintBanner();
                Console.WriteLine("ERROR: The previous entry was incorrectly formatted...\nPlease answer the question with the following inputs: \"y\",\"yes\",\"n\", or \"no\".");
                Console.WriteLine("Would you like to only include venues that offer takeout? (y/n)");
                takeout = Console.ReadLine().ToLower();
            }
            PrintBanner();
            //Ask if they only want to see restaurants that are currently open
            Console.WriteLine("Would you like to only search for venues that are currently open? (y/n)");
            var openNow = Console.ReadLine().ToLower();
            while (openNow != "y" && openNow != "yes" && openNow != "n" && openNow != "no")
            {
                PrintBanner();
                Console.WriteLine("ERROR: The previous entry was incorrectly formatted...\nPlease answer the question with the following inputs: \"y\",\"yes\",\"n\", or \"no\".");
                Console.WriteLine("Would you like to only search for venues that are currently open? (y/n)");
                openNow = Console.ReadLine().ToLower();
            }
            PrintBanner();
            //Sort by rating, highest to lowest
            results.Sort((y, x) => x.rating.CompareTo(y.rating));
            //Clean Array Data
            //Takeout only
            if (takeout == "y" || takeout == "yes")
            {
                results.RemoveAll(p => !p.types.Any("meal_takeaway".Contains));
                searchString += "\nOffers Takeout: YES";
            }
            else
            {
                searchString += "\nOffers Takeout: NO";
            }
            //Only open now
            if (openNow == "y" || openNow == "yes")
            {
                results.RemoveAll(p => p.opening_hours.open_now == false);
                searchString += "\nOpen Now: YES";
            }
            else
            {
                searchString += "\nOffers Takeout: NO";
            }
            Console.WriteLine(searchString + "\n");
            return results;
        }
        private List<PlacesData.Result> PlacesDataRequest(string request)
        {
            string baseRequest = request;
            string currentHTMLRequest = baseRequest;
            List<PlacesData.Result> results = new List<PlacesData.Result>();

            WebClient webClient = new WebClient();
            string jsonData = webClient.DownloadString(currentHTMLRequest);
            PlacesData rootJObject = JsonConvert.DeserializeObject<PlacesData>(jsonData);
            if(rootJObject.results != null)
            {
                results.AddRange(rootJObject.results);
            }
            string nextPageToken = rootJObject.next_page_token;
            while (rootJObject.next_page_token != null)
            {
                currentHTMLRequest = baseRequest + "&pagetoken=" + nextPageToken;
                do
                {
                    jsonData = webClient.DownloadString(currentHTMLRequest);
                    Thread.Sleep(500);
                } while (jsonData.Contains("INVALID_REQUEST"));

                rootJObject = JsonConvert.DeserializeObject<PlacesData>(jsonData);
                results.AddRange(rootJObject.results);

                nextPageToken = rootJObject.next_page_token;
            }
            return results;
        }
        private double SearchRadius()
        {
            PrintBanner();
            Console.WriteLine("Please enter the maximum number of miles away from your current location that the destination may be...");
            var numMiles = Console.ReadLine();
            double miles = 0;
            try
            {
                miles = Convert.ToInt32(numMiles);
            }
            catch(Exception e)
            {
            }
            while (double.IsNaN(miles) || miles == 0)
            {
                PrintBanner();
                Console.WriteLine("ERROR: The previous entry was not a number of miles, please only use whole numbers or decimals for your input.");
                numMiles = Console.ReadLine();
                try
                {
                    miles = Convert.ToInt32(numMiles);
                }
                catch(Exception e)
                {
                    miles = 0;
                }
            }
            double searchRadius = miles * 1609.344;
            return searchRadius;
        }
        private string SearchKey()
        {
            PrintBanner();
            Console.WriteLine("Please enter the search term you would like to base the search on...");
            Console.WriteLine("You may leave this blank to return all applicable options.");
            Console.WriteLine("\nEXAMPLES:\n[Food] Burgers\n[Type] Store\n[Grocery] Kroger\n[Name] Advanced Auto Parts\n\n");
            string searchKey = Console.ReadLine();
            return searchKey;
        }
        private void AppLoading()
        {
            for (int i = 0; i < 5; ++i)
            {
                PrintBanner();
                Console.Write(".");
                Thread.Sleep(100);
                Console.Write(".");
                Thread.Sleep(100);
                Console.Write(".");
                Thread.Sleep(100);
                Console.Write(".");
                Thread.Sleep(100);
                Console.Write(".");
                Thread.Sleep(100);
                Console.Write(".");
                Thread.Sleep(100);
            }
        }
        private void PrintBanner()
        {
            Console.Clear();
            Console.WriteLine(@"                   ########################################");
            Console.WriteLine(@"                   ##     __ __                          ##");
            Console.WriteLine(@"                   ##    / // /_ __ ___  ___ _______ __  ##");
            Console.WriteLine(@"                   ##   / _  /\ \ // _ \/ _ `/ __/ // /  ##");
            Console.WriteLine(@"                   ##  /_//_//_\_\/_//_/\_, /_/  \_, /   ##");
            Console.WriteLine(@"                   ##                  /___/    /___/    ##");
            Console.WriteLine(@"                   ##                                    ##");
            Console.WriteLine(@"                   ########################################");
            Console.WriteLine();
            Console.WriteLine();

        }
    }
}
