using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyListingGrabber
{
    class CurrencyListingResult
    {
        public string AccountName;
        public string CharacterName;
        public string League;
        public string ItemName;
        public int sellCurrency;
        public Decimal sellValue;
        public int buyCurrency;
        public Decimal buyValue;
        public int stock;
    }

    class Grabber
    {
        public static int MAX_BYTES = 1000000;
        public static int MAX_LISTINGS = 1000;

        public static CurrencyListingResult[] CurrencyResultList = new CurrencyListingResult[MAX_LISTINGS];

        public static string GetSubstring(string l_Source, string l_FindCharacter)
        {
            if (!l_Source.Contains(l_FindCharacter)) return string.Empty;
            int End = l_Source.IndexOf(l_FindCharacter, 0) + l_FindCharacter.Length;
            Console.WriteLine(End);
            return l_Source.Substring(0, End - 1);
        }

        public static async Task WriteFile(string name, string text)
        {
            using (StreamWriter file = new StreamWriter(name, append: true))
                await file.WriteLineAsync(text);
        }

        public static int GetItemMapping(string itemName) //todo: finish this for rest of items in currency market
        {
            int mapping = 0;

            switch (itemName)
            {
                case "Orb of Alteration":
                    mapping = 1;
                    break;

                case "Orb of Fusing":
                    mapping = 2;
                    break;

                case "Orb of Alchemy":
                    mapping = 3;
                    break;

                case "Chaos Orb":
                    mapping = 4;
                    break;

                case "Gemcutter's Prism":
                    mapping = 5;
                    break;

                case "Exalted Orb":
                    mapping = 6;
                    break;

                case "Chromatic Orb":
                    mapping = 7;
                    break;

                case "Jeweller's Orb":
                    mapping = 8;
                    break;

                case "Orb of Chance":
                    mapping = 9;
                    break;

                case "Cartographer's Chisel":
                    mapping = 10;
                    break;

                case "Orb of Scouring":
                    mapping = 11;
                    break;

                case "Blessed Orb":
                    mapping = 12;
                    break;

                case "Orb of Regret":
                    mapping = 13;
                    break;

            };

            return mapping;
        }

        public async void GetListings(string server, string itemNameWant, int itemQuantityWant, string itemNameHave)
        {
            ServicePointManager.ServerCertificateValidationCallback = new
                       RemoteCertificateValidationCallback //SSL hack
                       (
                           delegate { return true; }
                       );

            Uri WebsiteURI = new Uri("https://currency.poe.trade");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(WebsiteURI);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = true;

            byte[] ouat = new byte[MAX_BYTES];
            byte[] searchResult = new byte[MAX_BYTES];

            WebResponse r = request.GetResponse();
            int result1 = r.GetResponseStream().Read(ouat, 0, MAX_BYTES);

            // From byte array to string
            string s = System.Text.Encoding.UTF8.GetString(ouat, 0, ouat.Length);

            int itemMappingWant = GetItemMapping(itemNameWant);
            int itemMappingHave = GetItemMapping(itemNameHave);

            string searchQuery = "search?league=" + server + "&online=x&want=" + itemMappingWant + "&have=" + itemMappingHave;

            //REQUEST 2: SEARCH
            HttpWebRequest request2 = (HttpWebRequest)HttpWebRequest.Create(WebsiteURI + searchQuery);
            request2.Method = "GET";
            request2.ContentType = "application/x-www-form-urlencoded";
            request2.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request2.AllowAutoRedirect = true;

            ouat = new byte[MAX_BYTES];
            searchResult = new byte[MAX_BYTES];

            r = request2.GetResponse();
            result1 = r.GetResponseStream().Read(ouat, 0, MAX_BYTES);

            // From byte array to string
            s = System.Text.Encoding.UTF8.GetString(ouat, 0, ouat.Length);

            int readIndex = 0, endIndex = 0;

            int index_displayOffer = s.IndexOf("displayoffer \"", 0);
            int index_accountName = s.IndexOf("data-username", 0);
            int index_sellCurrency = s.IndexOf("data-sellcurrency", 0);
            int index_sellValue = s.IndexOf("data-sellvalue", 0); //quantity offered
            int index_buyCurrency = s.IndexOf("data-buycurrency", 0);
            int index_buyValue = s.IndexOf("data-buyvalue", 0);
            int index_ign = s.IndexOf("data-ign", 0);
            int index_stock = s.IndexOf("data-stock", 0);

            await WriteFile("currency_out.txt", "Results for " + itemNameWant + " in " + server + ":");

            while (index_displayOffer > 0)
            {
                CurrencyListingResult result = new CurrencyListingResult();

                index_displayOffer = s.IndexOf("displayoffer \"", readIndex);
                endIndex = s.IndexOf("\"", readIndex); //get closing quotation for end of line...

                if(index_displayOffer > 0)
                {
                    index_accountName = s.IndexOf("data-username", index_displayOffer);
                    readIndex = index_accountName + "data-username".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string username = s.Substring(readIndex, endIndex - readIndex);
                    result.AccountName = username;

                    index_sellCurrency = s.IndexOf("data-sellcurrency", index_displayOffer);
                    readIndex = index_sellCurrency + "data-sellcurrency".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string ItemName = s.Substring(readIndex, endIndex - readIndex);
                    result.ItemName = ItemName; //will be a number

                    index_sellValue = s.IndexOf("data-sellvalue", readIndex);
                    readIndex = index_sellValue + "data-sellvalue".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string sellValue = s.Substring(readIndex, endIndex - readIndex);

                    index_buyCurrency = s.IndexOf("data-buycurrency", index_sellValue);
                    readIndex = index_buyCurrency + "data-buycurrency".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string buyCurrency = s.Substring(readIndex, endIndex - readIndex);


                    index_buyValue = s.IndexOf("data-buyvalue", index_buyCurrency);
                    readIndex = index_buyValue + "data-buyvalue".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string buyValue = s.Substring(readIndex, endIndex - readIndex);

                    index_ign = s.IndexOf("data-ign", index_buyValue);
                    readIndex = index_ign + "data-ign".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string IGN = s.Substring(readIndex, endIndex - readIndex);
                    result.CharacterName = IGN;

                    index_stock = s.IndexOf("data-stock", index_ign);
                    readIndex = index_stock + "data-stock".Length + 2;
                    endIndex = s.IndexOf("\"", readIndex);
                    string stock = s.Substring(readIndex, endIndex - readIndex);

                    readIndex = index_stock + 12;

                    result.League = server;
                    result.buyCurrency = Convert.ToInt32(GetItemMapping(itemNameWant));  
                    result.buyValue = Convert.ToDecimal(buyValue);                     
                    result.stock = Convert.ToInt32(stock);
                    result.sellCurrency = Convert.ToInt32(GetItemMapping(itemNameHave));
                    result.sellValue = Convert.ToDecimal(sellValue);

                    decimal desiredQuantity = 1;
                    decimal costPerUnit = result.buyValue / result.sellValue;
                    decimal totalCost = desiredQuantity * costPerUnit;

                    string whisper = CreateWhisper(IGN, server, itemNameWant, desiredQuantity, itemNameHave, totalCost );
                   
                    string WriteFileStr = "[" + server + "] " + stock + "x " + itemNameWant + " for " + sellValue + " " + itemNameHave + ", from IGN " + IGN + "(" + username + "), Cost per unit: " + costPerUnit;

                    await WriteFile("currency_out.txt", WriteFileStr);
                    await WriteFile("whispers.txt", whisper);

                    Console.WriteLine("Listing: SellCurrency {0}, SellValue {1}, BuyValue {2}, BuyCurrency {3}, IGN: {4}, Stock: {5}, CPU: {6}", ItemName, sellValue, buyValue, buyCurrency, IGN, stock, costPerUnit);
                }        
            }
        }

        public static string CreateWhisper(string CharacterName, string League, string ItemName, decimal ItemQuantity, string Buyout, decimal BuyUnitQuantity)
        {
            string Whisper = "@" + CharacterName + " ";
            Whisper += "Hi, I would like to buy your " + ItemQuantity + " " + ItemName + " ";
            Whisper += "for my " + BuyUnitQuantity + " " +  Buyout + " ";
            Whisper += "in " + League + ".";

            return Whisper;
        }

        public void ReadFileAndGetListings(string fileName)
        {
            string itemNameHave = string.Empty;
            string itemNameWant = string.Empty;
            string server = string.Empty;

            string[] lines = System.IO.File.ReadAllLines(@fileName);
            int counter = 0;

            // Display the file contents by using a foreach loop.
            foreach (string line in lines)
            {
                if (counter == 0)
                {
                    server = line;
                }
                else if (counter == 1)
                {
                    itemNameHave = line;
                }
                else if (counter == 2)
                {
                    itemNameWant = line;
                }

                counter += 1;
            }

            if (lines.Length == 0)
            {
                Console.WriteLine("Could not read file! Please check your input file/format and try again.");
                return;
            }

            GetListings(server, itemNameWant, 1, itemNameHave);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Grabber g = new Grabber();

            if (args.Length == 1)
            {
                g.ReadFileAndGetListings(args[0]);
            }
            else
            {
                Console.WriteLine("Usage (input file): ./ListingGrabber <fileName>");
          
                g.GetListings("Scourge", "Chaos Orb", 1, "Orb of Alchemy"); //test example
            }

        }
    }
}
