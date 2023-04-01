namespace RabbitMQ_Trader
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Channels;

    internal class Program
    {
        
        static void Main()
        {
            //initialize variables
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            string exchangeCode = "Orders"; //not found            
            bool FINISHED = false;
            string input = "";                      

            //connect and join
            ConnectionSetup(exchangeCode, channel);           
            Console.Clear();

           
            //loop for sending and receiving messages
            while (!FINISHED)
            {                
                if (input != "EXIT")
                {
                    ConsolePrompts(channel,exchangeCode);
                }
                else
                {                   
                    FINISHED = true;
                }

            }

        }

        static void ConnectionSetup(string exchangeCode, IModel channel)
        {

            channel.ExchangeDeclare(exchange: exchangeCode, type: ExchangeType.Fanout);

            // declare a server-named queue
            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                exchange: exchangeCode,
                routingKey: string.Empty);

            //consuumer listener
            EventingBasicConsumer consumer = new (channel);       

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        static void ConsolePrompts(IModel channel, string exchangeCode)
        {
            //Console.WriteLine("\n\n\tConnecting...\n");
            
            bool buyOrSell;

            Console.WriteLine("\n\n\tEnter Username / Trader ID (no spaces): ");
            string username = Console.ReadLine()!;

            Console.WriteLine("\n\n\tDo you wish to BUY or SELL?: ");
            string choice = Console.ReadLine()!;

            string word1 = "";
            string word2 = "";

            // buy choice
            if (choice.ToLower() == "buy") // could force that into lowercase probably
            {
                buyOrSell = true;
                word1 = "buy";
                word2 = "pay";

            }
            else if (choice.ToLower() == "sell" )
            {
                buyOrSell = false;
                word1 = "sell";
                word2 = "charge";
            }
            else
            {
                Console.WriteLine("\n\tInvalid input");
                Console.WriteLine("\n\tPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            

            // read from exchange? might need a list of stocks (can expand functionality for multiple stocks)
            try
            {
                Console.WriteLine($"\n\tWhat would you like to {word1}?: "); // can make stocks have a 3-4 letter code for simplicity
                string stockChoice = Console.ReadLine()!;

                Console.WriteLine($"\n\tHow many shares would you like to {word1}? (I.E. 100): "); // supposed to be a fixed quantity but i left it variable
                int Qty = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine($"\n\thow much will you {word2} per share?: ");
                double price = Convert.ToDouble(Console.ReadLine());

                Console.WriteLine($"\n\n\t{word1}ing: " + Qty + "x shares of " + stockChoice + ", at $" + price + ".");

                Exchange_Order newOrder = new Exchange_Order(buyOrSell, Qty, username, price, stockChoice);
                SendOrder(channel, exchangeCode, newOrder);


                Console.WriteLine("\n\tPress Enter to continue...");
                Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("\n\tInvalid input");
                Console.WriteLine("\n\tPress Enter to continue...");
                Console.ReadLine();

            }
            Console.Clear();


        }


        static void SendOrder(IModel channel, string exchangeCode, Exchange_Order newOrder)
        {
                       
            var encoded_message = newOrder.NewMessage();

            channel.BasicPublish(exchange: exchangeCode,
                routingKey: string.Empty,
                basicProperties: null,
                body: encoded_message);

        }

    }

}