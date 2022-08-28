using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Application2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly string myClientId;

        MqttFactory mqttFactory;
        IMqttClient client;
        IMqttClientOptions options;

        // Observabilna kolekcija, dole ima objasnjenje zasto
        private readonly ObservableCollection<MessageItem> items;
        string topic;
        MqttTopicFilter topicFilter;

        [Obsolete]
        public MainWindow()
        {
            mqttFactory = new MqttFactory();
            client = mqttFactory.CreateMqttClient();

            myClientId = Guid.NewGuid().ToString();

            options = new MqttClientOptionsBuilder()
                        .WithClientId(myClientId)
                        .WithTcpServer("test.mosquitto.org", 1883)
                        .Build();

            client.ConnectAsync(options);
            
            client.UseConnectedHandler(e =>
            {
                if (client.IsConnected)
                {
                    MessageBox.Show("Successfully connected.222");
                    Log.Logger.Information("Successfully connected.222");
                }
                
            });

            items = new ObservableCollection<MessageItem>();

            InitializeComponent();
        }

        [Obsolete]
        private void button_Click_Connect(object sender, RoutedEventArgs e)
        {
            topic = tbTopic.Text;
            
            topicFilter = new TopicFilterBuilder()
                                .WithTopic(topic)
                                .Build();
            client.SubscribeAsync(topicFilter);

            MessageBox.Show("Konektovani ste na topic: " + topic);

            client.UseApplicationMessageReceivedHandler(e =>
            {
                // Posto je listbox UI komponenta, ona zivi na Main Thread-u i nije moguce da joj
                // se pristupi iz drugih Thread-ova. Koriscenje Dispatcher-a omogucava da se pristupi
                // UI elementu. U WPF-u je bitno da se koriste Observable kolekcije kako bi se promene
                // emitovale na UI elementima. items je tipa ObservableCollection
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                if (payload != null && !string.Empty.Equals(payload))
                {
                    var splittedMessage = payload.Split(';');
                    var clientId = splittedMessage[0];
                    var message = splittedMessage[1];

                    if (clientId.Equals(myClientId))
                    {
                        return;
                    }

                    Action invokeAction = new Action(() =>
                    {
                        items.Add(new MessageItem() { clientId = clientId, payload =  message}); //ne koristi e.clientId, to je id sendera
                        listBox2.ItemsSource = items;
                    });

                    listBox2.Dispatcher.BeginInvoke(invokeAction);
                }

                
            });

        }

        private void button_Click_Disconnect(object sender, RoutedEventArgs e)
        {
            client.DisconnectAsync();
            client.UseDisconnectedHandler(e =>
            {
                Log.Logger.Information("Successfully disconnected.");
            });
        }

        private void button_Click_Push(object sender, RoutedEventArgs e)
        {
            var messageToSend = tbPayload.Text;
            PublisHMessageAsync(client, messageToSend, topic);
        }

        private void PublisHMessageAsync(IMqttClient client, string message, string topic)
        {
            string messagePayload = $"{myClientId};{message}";

            var messageObj = new MqttApplicationMessageBuilder()
                                    .WithTopic(topic)
                                    .WithPayload(messagePayload)
                                    .WithAtLeastOnceQoS()
                                    .Build();

            if (client.IsConnected)
            {
                client.PublishAsync(messageObj);
            }
        }
    }
}
