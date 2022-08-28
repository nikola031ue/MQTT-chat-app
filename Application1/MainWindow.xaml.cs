using Application2;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Application1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MqttFactory mqttFactory;
        IMqttClient client;
        IMqttClientOptions options;
        MqttTopicFilter topicFilter;

        private string topic;
        string messageToSend;
        private readonly string myClientId;

        private readonly ObservableCollection<MessageItem> items;

        public MainWindow()
        {
            mqttFactory = new MqttFactory();

            myClientId = Guid.NewGuid().ToString();
            MessageBox.Show(myClientId);

            client = mqttFactory.CreateMqttClient();
            options = new MqttClientOptionsBuilder()
                        .WithClientId(myClientId)
                        .WithTcpServer("test.mosquitto.org", 1883)
                        .Build();
            client.ConnectAsync(options);

            client.UseConnectedHandler(e =>
            {
                if (client.IsConnected)
                {
                    MessageBox.Show("Connected to brooker1111");
                    Log.Logger.Information("Successfully connected1111.");
                }

            });

            items = new ObservableCollection<MessageItem>();

            InitializeComponent();
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
            messageToSend = tbPayload.Text;
            PublisHMessageAsync(client, messageToSend, topic);

        }

        private void button_Click_Connect(object sender, RoutedEventArgs e)
        {
            topic = tbTopic.Text;
            MessageBox.Show("Konektovani ste na topic: " + topic);

            topicFilter = new TopicFilterBuilder()
                                .WithTopic(topic)
                                .Build();

            client.SubscribeAsync(topicFilter);

            client.UseApplicationMessageReceivedHandler(e =>
            {
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
                        items.Add(new MessageItem() { clientId = clientId, payload = message });
                        listBox1.ItemsSource = items;
                    });

                    listBox1.Dispatcher.BeginInvoke(invokeAction);
                }

            });
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
