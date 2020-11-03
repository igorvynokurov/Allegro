namespace OrdersSearchService.Data.Config
{
    public class RabbitConfig
    {
        public string RabbitUserName { get; set; }
        public int RabbitPort { get; set; }
        public string RabbitHost { get; set; }
        public string RabbitPassword { get; set; }

        public RabbitConfig()
        {
            RabbitUserName = "rabbitmq";
            RabbitPassword = "8GAnxvz95Dkx7Mac";
            RabbitPort = 5672;
            RabbitHost = "154.27.80.183";
        }
    }
}
