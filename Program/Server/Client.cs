using System.Net;
using System.Net.Sockets;
using System.Text;
using Butterfly;

namespace Server
{
    public sealed class Client : Controller.Board.LocalField<TcpClient>, Client.ICommunication
    {
        public const string SET_NAME = "set_name";
        public const string CREATING_ROOM = "creating_room";
        public const string CONNECTING_TO_ROOM = "connecting_to_room";
        public const string DISCONNECTING_ROOM = "disconnecting_room";
        public const string SEND_MESSAGE_TO_ROOM = "send";
        public const string DESTROY_ROOM = "destroy_room";
        public const string DISCONNECT = "disconnect";

        private Dictionary<string, Room.ICommunication> _rooms =
            new Dictionary<string, Room.ICommunication>();

        private NetworkStream _stream;

        private IPEndPoint _ipEndPoint;

        private const int BUFFER_SIZE = 65545;

        private bool _isRunning = true;

        private IInput<string, string, ICommunication> i_creatingRoom;
        private IInput<string, string, ICommunication> i_connectingRoom;
        private IInput<string, string> i_sendToRoomMessage;
        private IInput<string, string> i_disconnetionRoom;
        private IInput<string, Client> i_addClient;
        private IInput<string> i_sendToNetwork;
        private IInput<string> i_removeClient;
        private IInput i_disconnection_rooms;
        private IInput<string> i_checkName;

        public string Name { private set; get; } = "";

        void Construction()
        {
            input_to(ref i_sendToNetwork, Header.SERVER_SEND_NETWORK_EVENT, Send);

            send_message(ref i_addClient, ClientsManager.SUBSCRIBE_CLIENT);
            send_message(ref i_removeClient, ClientsManager.UNSUBSCRIBE_CLIENT);

            send_echo_1_2<string, string, bool>(ref i_checkName, ClientsManager.CHECK_NAME)
                .output_to((name, isResult) =>
                {
                    if (isResult)
                    {
                        Name = name;

                        i_sendToNetwork.To
                            ($"{ResponceFromTheServer.SUCCSESS_SET_NAME}{_.SPLIT_MESSAGE}" +
                                $"{name}{_.END_MESSAGE}");
                    }
                    else i_sendToNetwork.To
                        ($"{ResponceFromTheServer.UNSUCCSESS_SET_NAME}{_.SPLIT_MESSAGE}" +
                            $"{name}{_.END_MESSAGE}");
                });

            send_echo_3_3<string, string, Client.ICommunication, string, bool, Room.ICommunication>
                (ref i_creatingRoom, RoomManager.CREATING_ROOM)
                    .output_to((roomName, isCreating, roomCommuniction) =>
                    {
                        if (isCreating)
                        {
                            _rooms.Add(roomName, roomCommuniction);

                            i_sendToNetwork.To
                                ($"{ResponceFromTheServer.SUCCSESS_ROOM_CREATE}{_.SPLIT_MESSAGE}" +
                                    $"{roomName}{_.END_MESSAGE}");
                        }
                        else i_sendToNetwork.To
                            ($"{ResponceFromTheServer.UNSUCCSESS_ROOM_CREATE}{_.SPLIT_MESSAGE}" +
                                $"{roomName}{_.END_MESSAGE}");
                    });

            send_echo_3_3<string, string, Client.ICommunication, string, string, Room.ICommunication>
                (ref i_connectingRoom, RoomManager.CONNECTION_TO_ROOM)
                    .output_to((roomName, error, roomCommuniction) =>
                    {
                        if (error == "")
                        {
                            _rooms.Add(roomName, roomCommuniction);

                            i_sendToNetwork.To
                                ($"{ResponceFromTheServer.SUCCSESS_CONNECT_TO_ROOM}{_.SPLIT_MESSAGE}" +
                                    $"{roomName}{_.END_MESSAGE}");
                        }
                        else i_sendToNetwork.To
                            ($"{ResponceFromTheServer.UNSUCCSESS_CONNECT_TO_ROOM}{_.SPLIT_MESSAGE}" +
                                $"{roomName}{_.SPLIT_MESSAGE}{error}{_.END_MESSAGE}");
                    });

            send_echo_2_2<string, string, string, string>
                (ref i_disconnetionRoom, RoomManager.DISCONNECTING_ROOM)
                    .output_to((roomName, error) =>
                    {
                        if (error == "")
                        {
                            if (_rooms.Remove(roomName))
                            {
                                i_sendToNetwork.To
                                    ($"{ResponceFromTheServer.SUCCSESS_DISCONNECT_ROOM}{_.SPLIT_MESSAGE}" +
                                        $"{roomName}{_.END_MESSAGE}");
                            }
                            else throw new Exception();
                        }
                        else i_sendToNetwork.To
                                ($"{ResponceFromTheServer.UNSUCCSESS_DISCONNECT_ROOM}{_.SPLIT_MESSAGE}" +
                                    $"{roomName}{_.SPLIT_MESSAGE}{error}{_.END_MESSAGE}");
                    });

            input_to<string, string>(ref i_sendToRoomMessage, Header.SERVER_ROOM_EVENT, (roomName, message) =>
            {
                if (_rooms.TryGetValue(roomName, out Room.ICommunication roomCommunication))
                {
                    roomCommunication.Send(Name, message);
                }
                else i_sendToNetwork.To
                       ($"{ResponceFromTheServer.UNSUCCSESS_SENDING_MESSAGE_TO_ROOM}{_.SPLIT_MESSAGE}"
                           + $"{roomName}{_.SPLIT_MESSAGE}Вы не подключились к данной комнате.{_.SPLIT_MESSAGE}" +
                               $"{message}{_.END_MESSAGE}");
            });

            input_to_0_2<string, string[]>(ref i_disconnection_rooms, Header.SERVER_ROOM_EVENT, (@return) =>
            {
                string[] roomsName = _rooms.Keys.ToArray();

                @return.To(Name, roomsName);
            })
            .send_message_to(RoomManager.DISCONNECTING_ROOMS);

            add_event(Header.SERVER_RECEIVE_NETWORK_EVENT, () =>
            {
                if (_isRunning == false) return;

                try
                {
                    int count = Field.Available;
                    if (count > 0)
                    {
                        byte[] buffer = new byte[count];
                        _stream.Read(buffer, 0, count);
                        Command(buffer);
                    }
                }
                catch { destroy(); }
            });
        }

        void Command(byte[] buffer)
        {
            if (buffer.Length == 0) return;

            string[] messages = Encoding.UTF8.GetString(buffer).Split(_.END_MESSAGE);

            foreach (string m in messages)
            {
                if (m == "") continue;

                string[] commands = m.Split(_.SPLIT_MESSAGE);

                if (commands.Length == 0) throw new Exception();

                if (commands.Length > 1)
                {
                    switch (commands[0])
                    {
                        case CREATING_ROOM:

                            if (commands[1] != "")
                            {
                                i_creatingRoom.To(commands[1], Name, this);
                            }
                            else throw new Exception();

                            break;

                        case SEND_MESSAGE_TO_ROOM:

                            if (commands.Length == 3)
                            {
                                i_sendToRoomMessage.To(commands[1], commands[2]);
                            }
                            else throw new Exception();

                            break;

                        case CONNECTING_TO_ROOM:
                            i_connectingRoom.To(commands[1], Name, this);
                            break;

                        case DISCONNECTING_ROOM:
                            i_disconnetionRoom.To(commands[1], Name);
                            break;

                        case SET_NAME:
                            if (Name == "") i_checkName.To(commands[1]);
                            break;
                    }
                }
                else if (commands.Length == 1)
                {
                    if (commands[0] == DISCONNECT)
                    {
                        destroy();
                    }
                    else throw new Exception();
                }
                else throw new Exception();
            }
        }

        void ICommunication.Send(string roomName, string senderClientName, string message)
        {
            if (message == Room.DISCONNECT_CLIENT)
            {
                if (_rooms.Remove(roomName))
                {
                    i_sendToNetwork.To
                        ($"{ResponceFromTheServer.DICONNECT_ROOM}{_.SPLIT_MESSAGE}{roomName}" +
                            $"{_.SPLIT_MESSAGE}{senderClientName}{_.END_MESSAGE}");
                }
                else throw new Exception();
            }
            else i_sendToNetwork.To
                ($"{ResponceFromTheServer.RECEIVE_ROOM_MESSAGE}{_.SPLIT_MESSAGE}" +
                    $"{roomName}{_.SPLIT_MESSAGE}{senderClientName}{_.SPLIT_MESSAGE}{message}{_.END_MESSAGE}");
        }

        void Start()
        {
            SystemInformation
                ("Connect client...", ConsoleColor.Green);

            i_addClient.To(GetKey(), this);
        }

        void Send(string message)
        {
            if (message == "" || _isRunning == false) return;

            if (Field.Connected)
            {
                try
                {
                    _stream.Write(Encoding.UTF8.GetBytes(message));
                }
                catch { destroy(); }
            }
            else destroy();
        }

        void Configurate()
        {
            try
            {
                _stream = Field.GetStream();
            }
            catch { destroy(); }
        }

        void Destruction() => _isRunning = false;

        void Stop()
        {
            if (StateInformation.IsCallConfigurate)
            {
                i_disconnection_rooms.To();
            }

            if (StateInformation.IsCallStart)
            {
                i_removeClient.To(GetKey());
            }
            else ClientsManager.DecrementCreateClient();

            if (Field != null)
            {
                try
                {
                    if (_stream != null)
                    {
                        _stream.Close();
                        _stream.Dispose();
                    }

                    Field.GetStream().Close();
                    Field.Close();
                    Field.Dispose();
                }
                catch { }
            }

            SystemInformation
                ("Disconnect client.", ConsoleColor.Green);
        }

        public interface ICommunication
        {
            void Send(string roomName, string senderClientName, string message);
        }
    }
}
