using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spryng.Models.Sms
{
    public class SmsRequest
    {
        /// <summary>
        /// Unique reference for delivery reports
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// A list of at least 1 and a maximum of 1000 MSISDN-numeric destination numbers (international format without leading "00" or "+")
        /// </summary>
        public string[] Destinations { get; set; }

        /// <summary>
        /// A Numeric or Alphanumeric field that specifies the originator.
        /// <para>Maximum length of 14 for Numeric and 11 for Alphanumeric.</para>
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Content of the SMS in GSM 7-bit alphabet encoding.
        /// <para>Maximum length of 160 chars for a single text message and up to 612 characters if long messages are allowed.</para>
        /// <para>When using long sms the system will automatically divide your message into messages up to 153 chars per sms.</para>
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Reference tag can be used to create a filter in statistics
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// To select the Spryng Business, Spryng Economy or Specific User route.
        /// </summary>
        /// <value>BUSINESS</value>
        public string Route { get; set; }

        /// <summary>
        /// If you wish to send Long SMS
        /// </summary>
        /// <remarks>
        /// When using long sms the system will automatically devide your message into messages up to 153 characters per SMS.
        /// </remarks>
        /// <value>False</value>
        public bool AllowLong { get; set; }

        public SmsRequest()
        {
            AllowLong = false;
            Route = SpryngRoute.BUSINESS;
        }
    }
}
