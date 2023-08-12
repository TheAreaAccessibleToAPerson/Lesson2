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
        public const string SEND_MESSAGE_TO_ROOM = "creating_message_to_room";
        public const string DESTROY_ROOM = "destroy_room";

        private Dictionary<string, Room.ICommunication> _room =
            new Dictionary<string, Room.ICommunication>();

        private NetworkStream _stream;

        private IPEndPoint _ipEndPoint;

        private const int BUFFER_SIZE = 65545;

        private bool _isRunning = true;

        private IInput<string, string, ICommunication> _inputToCreatingRoom;
        private IInput<string, Client> _inputToAddClient;
        private IInput<string> _inputToRemoveClient;
        private IInput<string> _inputToCheckName;
        private IInput<string> _inputToSend;

        public string Name { private set; get; } = "";

        void Construction()
        {
            send_echo_1_2<string, string, bool>(ref _inputToCheckName, ClientsManager.CHECK_NAME)
                .output_to((name, isResult) =>
                {
                    if (isResult)
                    {
                        Name = name;

                        _inputToSend.To($"{ResponceFromTheServer.SUCCSESS_SET_NAME}{_.SPLIT_MESSAGE}" +
                            $"{name}{_.SPLIT_MESSAGE}");
                    }
                    else _inputToSend.To($"{ResponceFromTheServer.UNSUCCSESS_SET_NAME}{_.SPLIT_MESSAGE}" +
                        $"{name}{_.SPLIT_MESSAGE}");
                });

            input_to(ref _inputToSend, Header.EVENT_1, Send);

            send_message(ref _inputToAddClient, ClientsManager.SUBSCRIBE_CLIENT);
            send_message(ref _inputToRemoveClient, ClientsManager.UNSUBSCRIBE_CLIENT);

            send_echo_3_3<string, string, Client.ICommunication, string, bool, Room.ICommunication>
                (ref _inputToCreatingRoom, RoomManager.CREATING_ROOM)
                    .output_to((roomName, isCreating, roomCommuniction) =>
                    {
                        if (isCreating)
                        {
                            _room.Add(roomName, roomCommuniction);

                            _inputToSend.To($"{ResponceFromTheServer.SUCCSESS_ROOM_CREATE}{_.SPLIT_MESSAGE}" +
                                $"{roomName}{_.END_MESSAGE}");
                        }
                        else _inputToSend.To
                            ($"{ResponceFromTheServer.UNSUCCSESS_ROOM_CREATE}{_.SPLIT_MESSAGE}" +
                                $"Комната с именем {roomName} уже создана.{_.END_MESSAGE}");
                    });

            add_event(Header.EVENT_1, () =>
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
                catch
                {
                    Destroy();
                }
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

                if (commands.Length > 1)
                {
                    switch (commands[0])
                    {
                        case CREATING_ROOM:

                            if (commands.Length == 2)
                            {
                                if (commands[1] != "")
                                {
                                    _inputToCreatingRoom.To(GetKey(), commands[1], this);
                                }
                                else
                                {
                                    SystemInformation
                                        ("creating_room:Error[Пришло пустое имя для комнаты]", System.ConsoleColor.Red);

                                    Destroy();
                                }
                            }
                            else
                            {
                                SystemInformation($"creating_room:Error[Пришло неверное количесво комманд, " +
                                    $"ожидали 2, получили {commands.Length}]", System.ConsoleColor.Red);

                                Destroy();
                            }
                            break;

                        case SET_NAME:

                            if (Name == "")
                                _inputToCheckName.To(commands[1]);

                            break;

                        default:

                            Send("creating_room - создание комнаты.");

                            break;
                    }
                }
                else Destroy();
            }
        }

        void ICommunication.Send(string senderName, string message)
        {
        }

        void Start()
        {
            SystemInformation("Connect client.", ConsoleColor.Green);

            _inputToAddClient.To(GetKey(), this);
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
                catch
                {
                    Destroy();
                }
            }
            else Destroy();
        }

        void Configurate()
        {
            try
            {
                _stream = Field.GetStream();
            }
            catch
            {
                Destroy();
            }
        }

        void Stop()
        {
            _isRunning = false;

            if (StateInformation.IsCallStart)
            {
                _inputToRemoveClient.To(GetKey());
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
                catch
                { }
            }

            SystemInformation("Disconnect client.", ConsoleColor.Green);
        }

        private void Destroy()
        {
            if (_isRunning)
            {
                _isRunning = false;

                destroy();
            }
        }

        public interface ICommunication
        {
            void Send(string senderName, string message);
        }
    }
}
