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
        
        static void Main(string[] args)
        {
            //initialize variables
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            string exchangeCode = "Orders"; //not found
            string username = "Default";
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
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {

                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] {message}");
                Console.WriteLine($"");
            };


            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        static void ConsolePrompts(IModel channel, string exchangeCode)
        {
            Console.WriteLine("\n\n\tConnecting...\n");


            string username;
            string choice;
            bool buyOrSell;

            Console.WriteLine("\n\n\tEnter Username / Trader ID (no spaces): ");
            username = Console.ReadLine();

            Console.WriteLine("\n\n\tDo you wish to BUY or SELL?: ");
            choice = Console.ReadLine();

            // buy choice
            if (choice == "BUY" || choice == "buy") // could force that into lowercase probably
            {
                buyOrSell = true;
                Console.Clear();

                // read from exchange? might need a list of stocks (can expand functionality for multiple stocks)

                Console.WriteLine("\n\tWhat would you like to buy?: "); // can make stocks have a 3-4 letter code for simplicity
                string stockChoice = Console.ReadLine();

                Console.WriteLine("\n\tHow many shares would you like to buy? (I.E. 100): "); // supposed to be a fixed quantity but i left it variable
                int Qty = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("\n\thow much will you pay per share?: ");
                double price = Convert.ToDouble(Console.ReadLine());

                Console.WriteLine("\n\n\tBuying: " + Qty + "x shares of " + stockChoice + ", at $" + price + ".");

                Exchange_Order newOrder = new Exchange_Order(buyOrSell,Qty,username,price);
                SendOrder(channel, exchangeCode, newOrder);

            }

            // sell?
            if (choice == "SELL" || choice == "sell")
            {
                buyOrSell = false;
                Console.Clear();

                Console.WriteLine("\n\tWhat do you wish to sell?: ");
                string stockChoice = Console.ReadLine();

                Console.WriteLine("\n\tHow many do you want to sell?: ");
                int Qty = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("\n\twhat price do you wish to sell at?: ");
                double price = Convert.ToDouble(Console.ReadLine());

                // confirm sale
                Console.WriteLine("\n\tSelling: " + Qty + "x shares of " + choice + ", at $" + price + ".");
                Exchange_Order newOrder = new Exchange_Order(buyOrSell, Qty, username, price);
                SendOrder(channel, exchangeCode, newOrder);
            }

            Console.WriteLine("\n\tPress any key to exit...");
            Console.ReadLine();

        }


        static void SendOrder(IModel channel, string exchangeCode, Exchange_Order newOrder)
        {
                       
            var encoded_message = newOrder.newMessage();

            channel.BasicPublish(exchange: exchangeCode,
                routingKey: string.Empty,
                basicProperties: null,
                body: encoded_message);

        }


    }

}