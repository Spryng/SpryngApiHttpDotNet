using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spryng;

namespace SpryngApiDotNet.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = GetInput("Username");
            string password = GetInput("Password", true);

            SpryngClient client = new SpryngClient(username, password);

            Console.WriteLine("Available Credits: {0}", client.GetCreditAmount());



            Console.ReadKey(true);
        }


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
