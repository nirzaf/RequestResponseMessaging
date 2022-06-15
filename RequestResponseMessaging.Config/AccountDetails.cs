using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestResponseMessaging.Config
{
    public class AccountDetails
    {

        //ToDo: Enter a valid Serivce Bus connection string
        public static string ConnectionString = "";

        public static string RequestQueueName = "requestqueue";
        public static string ResponseQueueName = "responsequeue";

    }
}
