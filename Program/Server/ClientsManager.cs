using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Server
{
    public sealed class ClientsManager : Controller
    {
        public const string ADD_CLIENT = "AddClientInClientsManager";
        public const string CHECK_NAME = "CheckName";
        public const string SUBSCRIBE_CLIENT = "SubscribeClient";
        public const string UNSUBSCRIBE_CLIENT = "UnsubscribeClient";

        private Dictionary<string, Server.Client> _clients
            = new Dictionary<string, Client>();

        private IInput<string, uint> _inputToReplaceEventTimeDelay;
        private int _countCreatingClient = 0;


        private static IInput s_decrementCreateClient;
        public static void DecrementCreateClient()
            => s_decrementCreateClient.To();

        void Construction()
        {
            obj<RoomManager>("RoomsManager");

            send_message(ref _inputToReplaceEventTimeDelay, Event.REPLACE_TIME_DELAY);

            input_to(ref s_decrementCreateClient, Header.WORK_WITCH_OBJECTS_EVENT, () =>
            {
                if ((--_countCreatingClient) == 0)
                    _inputToReplaceEventTimeDelay.To(Header.WORK_WITCH_OBJECTS_EVENT, 10);
            });

            listen_echo_1_2<string, string, bool>(CHECK_NAME)
                .output_to((name, @return) =>
                {
                    if (_clients.Any(client => client.Value.Name == name))
                    {
                        @return.To(name, false);
                    }
                    else @return.To(name, true);
                },
                Header.WORK_WITCH_OBJECTS_EVENT);

            listen_message<TcpClient>(ADD_CLIENT)
                .output_to((client) =>
                {
                    _countCreatingClient++;

                    _inputToReplaceEventTimeDelay.To(Header.WORK_WITCH_OBJECTS_EVENT, 0);

                    string name = $"{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}/" +
                        $"{((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString()}";

                    if (try_obj(name, out Server.Client createClient))
                        createClient.destroy();

                    obj<Client>(name, client);
                },
                Header.WORK_WITCH_OBJECTS_EVENT);

            listen_message<string, Server.Client>(SUBSCRIBE_CLIENT)
                .output_to((name, client) =>
                {
                    if ((--_countCreatingClient) == 0)
                        _inputToReplaceEventTimeDelay.To(Header.WORK_WITCH_OBJECTS_EVENT, 10);

                    _clients.Add(name, client);
                },
                Header.WORK_WITCH_OBJECTS_EVENT);

            listen_message<string>(UNSUBSCRIBE_CLIENT)
                .output_to((name) =>
                {
                    if (_clients.Remove(name) == false)
                        throw new Exception();
                },
                Header.WORK_WITCH_OBJECTS_EVENT);

        }
    }
}
