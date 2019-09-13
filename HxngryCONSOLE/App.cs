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
        public void InitApp(User user)
        {
            this.user = user;
            InitLocation();
            InitTwilio();
            InitPhoneContact();
        }
        public void MainLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Please select one of the following options to begin. . .");
                Console.WriteLine("[F] Food Search\n[G] Grocery Search\n[E] Entertainment Search\n[N] Search by Name\n[Q] To Quit");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if (userChoice.KeyChar == 'f' || userChoice.KeyChar == 'F')
                {
                    BuildFoodRequest();
                }
                else if (userChoice.KeyChar == 'g' || userChoice.KeyChar == 'G')
                {
                    //GrocerySearch();
                }
                else if (userChoice.KeyChar == 'e' || userChoice.KeyChar == 'E')
                {
                    //EntertainmentSearch();
                }
                else if(userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
                {
                    //NameSearch();
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
            Console.WriteLine("###############################");
            Console.WriteLine("###### Welcome to Hxngry ######");
            Console.WriteLine("###############################");
            Console.WriteLine();
            Console.WriteLine("Currently Hxngry will be conducting searches based on the location of your Public IPv4 address.\n");
            Console.WriteLine("IPv4 Address Location: " + this.user.City + ", " + this.user.State + " " + this.user.Postal + ", " + this.user.Country + ". (" + this.user.Latitude + ", " + this.user.Longitude + ")\n");
            while (true)
            {
                Console.WriteLine("[Y] Change your current address\n[N] Continue with the current address\n[Q] Quit Hxngry");
                ConsoleKeyInfo userChoice = Console.ReadKey();
                if(userChoice.KeyChar == 'y' || userChoice.KeyChar == 'Y')
                {
                    ChangeSearchParams();
                    Console.Clear();
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
                        Console.Clear();
                        Environment.Exit(0);
                    }
                }
                else if(userChoice.KeyChar == 'q' || userChoice.KeyChar == 'Q')
                {
                    Console.Clear();
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
        public void ChangeSearchParams()
        {
            Console.Clear();
            Console.WriteLine("You have selected to change your search address. . .");
            Console.WriteLine("Please follow the instructions and provide the necessary information to determine the search location. Press any key to continue. . .");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("We will require various parts of the address you'd like to search from. The format will be as such:");
            Console.WriteLine("Address Formatting Example: 2266 Candlyland Drive, CandyCity, CS --> <STREET ADDRESS>, <CITY>, <STATE ABBREVIATION>");
            Console.WriteLine("\nPlease enter your street address.\n");
            string addr = Console.ReadLine();
            this.user.SearchStreetAddress = addr;
            Console.Clear();
            Console.WriteLine("Address Formatting Example: 2266 Candlyland Drive, CandyCity, CS --> <STREET ADDRESS>, <CITY>, <STATE ABBREVIATION>");
            Console.WriteLine("\nPlease enter your city name.\n");
            addr = Console.ReadLine();
            this.user.SearchCity = addr;
            Console.Clear();
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
                Console.Clear();
                //Console.WriteLine("Please enter your Phone Number to be contacted via the Hxngry Service in the following format: +1###-###-####\n");
                //var phone = Console.ReadLine();
                //completedPhoneInit = this.SetPhoneContact(phone);
                var count = 0;
                var attempts = 0;
                while (completedPhoneInit == false)
                {
                    Console.Clear();
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
                Console.Clear();
                if (count == 0)
                {
                    Console.WriteLine("We must verify your number with Hxngry in order to send SMS messages.\nPlease follow the instructions provided on screen.\n\n Press any key to continue. . .");
                    Console.ReadKey();
                    VerifyCallerID();
                }
                else
                {
                    Console.WriteLine("Your number (" + this.user.Phone + ") is already verified with us, thank you for returning to Hxngry.");
                }
                Console.WriteLine("[Space Bar] To continue\n[R] To Resubmit Phone Number\n[Q] To Quit");
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
            Console.Clear();
            Console.WriteLine("Please enter the name of the number that Hxngry will be using. (Mike's Phone, John's iPhone, Mary, etc)");
            var numName = Console.ReadLine();
            var validationRequest = ValidationRequestResource.Create(
                friendlyName: numName,
                phoneNumber: new Twilio.Types.PhoneNumber(this.user.Phone)
            );
            Console.WriteLine("You will receive a call within the next minute, the call will request a verification code.\n");
            Console.WriteLine("This is your verification code: " + validationRequest.ValidationCode);
            Console.WriteLine("Once you have entered the verification code you may press any key to continue. . .");
            AppLoading();
            Console.Clear();
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
        //FOOD SEARCH METHODS
        public void BuildFoodRequest()
        {
            Console.Clear();
            double searchRadius = SearchRadius();
            string searchKey = SearchKey();
            string foodRequest = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + this.user.Latitude + "," + this.user.Longitude + "&radius=" + searchRadius + "&type=restaurant&keyword=" + searchKey + "&key=" + Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY");
            List<PlacesData.Result> results = PlacesDataRequest(foodRequest);
            
            results = CleanData(searchKey, searchRadius, results);
            PrintNearbyFoodData(results);
            ChoosePlace(results);
        }
        public void ChoosePlace(List<PlacesData.Result> results)
        {
            Console.WriteLine("\n\nSearch Successful. . .");
            Console.WriteLine("[N] Select Specific Restaurant\n[R] Conduct Another Search\n[Q] Quit");
            ConsoleKeyInfo userChoice = Console.ReadKey();
            if (userChoice.KeyChar == 'n' || userChoice.KeyChar == 'N')
            {
                Console.WriteLine("Please enter the name of the place you'd like more information on.\n");
                string place = Console.ReadLine();
                if (results.Exists(a => a.name == place))
                {
                    Console.Clear();
                    SelectedPlace placeInfo = new SelectedPlace();
                    int index = results.FindIndex(a => a.name == place);
                    string specificJson = placeInfo.InitPlaceInfo(results, index);
                    placeInfo = JsonConvert.DeserializeObject<SelectedPlace>(specificJson);
                    string[] printData = placeInfo.Info();
                    Console.WriteLine(printData[0]);
                    bool userReady = false;
                    while (userReady == false)
                    {
                        Console.WriteLine("[Space Bar] Display The Most Recent Reviews\n[Enter] Select This Restaurant and Notify via SMS\n[B] Return to Main Menu");
                        userChoice = Console.ReadKey();
                        if(userChoice.KeyChar == 'b' || userChoice.KeyChar == 'B')
                        {
                            Console.Clear();
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
                                Console.Clear();
                                break;
                            }
                            Console.ReadKey();
                            Console.Clear();
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
                Console.Clear();
                Environment.Exit(0);
            }

        }
        public void SelectPlace(SelectedPlace placeInfo)
        {
            Console.Clear();
            Console.WriteLine("You have selected " + placeInfo.result.name + ".");
            Console.WriteLine("Applicable SMS numbers will be notified shortly. . .");
            string messageBody = "You have selected to the restaurant: " + placeInfo.result.name + " located at " + placeInfo.result.vicinity + ".";
            //var message = MessageResource.Create(
            //    body: messageBody,
            //    from: new Twilio.Types.PhoneNumber("+15204629326"),
            //    to: new Twilio.Types.PhoneNumber("+18043705689")
            //);
            AppLoading();
            Console.Clear();
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
        public void PrintNearbyFoodData(List<PlacesData.Result> results)
        {
            Console.WriteLine("Search Complete. The results will be listed from highest to lowest rating below. . .\n");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("| {0,-45} | {1,-28} | {2,-8} | {3}", "Restaurant Name", "Ratings", "Distance", "Address");
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------");
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
                Console.WriteLine("| {0,-45} | {1,-28} | {2,-8} | {3}", name, reports, Math.Round(Calculations.distance(this.user.Latitude, this.user.Longitude, results[i].geometry.location.lat, results[i].geometry.location.lng, 'M'), 2).ToString() + " mi.", results[i].vicinity);
            }
            Console.WriteLine(" ------------------------------------------------------------------------------------------------------------------------------------------");
        }
        public List<PlacesData.Result> CleanData(string searchItem, double numMiles, List<PlacesData.Result> results)
        {
            Console.Clear();
            Console.WriteLine("Would you like to only include venues that offer takeout? (y/n)");
            var searchString = "Searching for " + searchItem + " within " + Math.Round(Convert.ToDouble(numMiles/1609.344),2).ToString() + " miles with the following parameters:";
            var takeout = Console.ReadLine().ToLower();
            while (takeout != "y" && takeout != "yes" && takeout != "n" && takeout != "no")
            {
                Console.Clear();
                Console.WriteLine("ERROR: The previous entry was incorrectly formatted...\nPlease answer the question with the following inputs: \"y\",\"yes\",\"n\", or \"no\".");
                Console.WriteLine("Would you like to only include venues that offer takeout? (y/n)");
                takeout = Console.ReadLine().ToLower();
            }
            Console.Clear();
            //Ask if they only want to see restaurants that are currently open
            Console.WriteLine("Would you like to only search for venues that are currently open? (y/n)");
            var openNow = Console.ReadLine().ToLower();
            while (openNow != "y" && openNow != "yes" && openNow != "n" && openNow != "no")
            {
                Console.Clear();
                Console.WriteLine("ERROR: The previous entry was incorrectly formatted...\nPlease answer the question with the following inputs: \"y\",\"yes\",\"n\", or \"no\".");
                Console.WriteLine("Would you like to only search for venues that are currently open? (y/n)");
                openNow = Console.ReadLine().ToLower();
            }
            Console.Clear();
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
        public List<PlacesData.Result> PlacesDataRequest(string request)
        {
            string baseRequest = request;
            string currentHTMLRequest = baseRequest;
            List<PlacesData.Result> results = new List<PlacesData.Result>();

            WebClient webClient = new WebClient();
            string jsonData = webClient.DownloadString(currentHTMLRequest);
            PlacesData rootJObject = JsonConvert.DeserializeObject<PlacesData>(jsonData);
            results.AddRange(rootJObject.results);
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
        public double SearchRadius()
        {
            Console.Clear();
            Console.WriteLine("Please enter the maximum number of miles away from your current location that the destination may be. . .");
            var numMiles = Console.ReadLine();
            double miles = Convert.ToInt32(numMiles);
            while (double.IsNaN(miles) || miles == 0)
            {
                Console.Clear();
                Console.WriteLine("ERROR: The previous entry was not a number of miles, please only use whole numbers or decimals for your input.");
                numMiles = Console.ReadLine();
                miles = Convert.ToInt32(numMiles);
            }
            double searchRadius = miles * 1609.344;
            return searchRadius;
        }
        public string SearchKey()
        {
            Console.Clear();
            Console.WriteLine("Please enter the search term you would like to base the search on. . .");
            Console.WriteLine("EXAMPLES:\n[Food] Burgers\n[Entertainment] Arcade\n[Grocery] Kroger\n[Name] Advanced Auto Parts\n\n");
            string searchKey = Console.ReadLine();
            return searchKey;
        }
        public void AppLoading()
        {
            for (int i = 0; i < 5; ++i)
            {
                Console.Clear();
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
    }
}
