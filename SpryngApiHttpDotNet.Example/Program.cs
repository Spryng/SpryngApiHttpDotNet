using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spryng;
using Spryng.Models.Sms;

namespace SpryngApiDotNet.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = GetInput("Username");
            string password = GetInput("Password", true);

            // Create a new SpryngHttpClient with the given Username and Password.
            SpryngHttpClient client = new SpryngHttpClient(username, password);

            // Get the amount of credits left in the account.
            Console.WriteLine("Available Credits: {0}", client.GetCreditAmount());

            // Make a new SmsRequest 
            SmsRequest request = new SmsRequest()
            {
                Destinations = new string[] { "31610831401", "31681338412" },
                Sender = GetInput("Sender"),
                Body = GetInput("Body")
            };


            // Execute the Sms Request.
            try
            {
                client.ExecuteSmsRequest(request);
                Console.WriteLine("SMS has been send!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Exception occured!\n{0}", ex.Message);
            }



            Console.ReadKey(true);
        }



        /// <summary>
        /// Simple helper method to get Console Input with a label.
        /// </summary>
        /// <param name="name">Label name</param>
        /// <param name="isHidden">If we should show the entered characters.</param>
        /// <returns></returns>
        private static string GetInput(string name, bool isHidden = false)
        {
            Console.Write("{0}: ", name);

            // If an input type is hidden make sure we do not display the entered characters.
            if (isHidden)
            {
                string result = "";
                ConsoleKeyInfo keyInfo;
                while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    result += keyInfo.KeyChar;
                }
                Console.WriteLine();
                return result;
            }

            return Console.ReadLine();
        }
    }
}
