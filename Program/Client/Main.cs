using System.Net;
using System.Net.Sockets;
using Butterfly;

public struct ResponceFromTheServer
{
    public const string SUCCSESS_SET_NAME = "succsess_set_name";
    public const string UNSUCCSESS_SET_NAME = "unsuccsess_set_name";

    public const string SUCCSESS_CONNECT_TO_ROOM = "succsess_connect_to_room";
    public const string UNSUCCSESS_CONNECT_TO_ROOM = "unsuccsess_connect_to_room";

    public const string SUCCSESS_DISCONNECT_ROOM = "succsess_disconnect_room";
    public const string UNSUCCSESS_DISCONNECT_ROOM = "unsuccsess_disconnect_room";

    public const string DICONNECT_ROOM = "disconnect_room";

    public const string RECEIVE_ROOM_MESSAGE = "receive_room_message";
    public const string UNSUCCSESS_SENDING_MESSAGE_TO_ROOM = "unsuccsess_sending_message_to_room_clients";

    public const string SUCCSESS_ROOM_CREATE = "success_room_create";
    public const string UNSUCCSESS_ROOM_CREATE = "unsuccess_rom_create";
}

namespace Client
{
    public sealed class Main : Controller.Board.LocalField<string[]>,
        ReadLine.IInformation
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private bool _isRunning = true;

        private const int BUFFER_SIZE = 65545;

        private const int DATA_COUNT = 3;
        private const int LOCAL_PORT_INDEX = 0;
        private const int REMOTE_ADDRESS_INDEX = 1;
        private const int REMOTE_PORT_INDEX = 2;

        public string Port {get => Field[LOCAL_PORT_INDEX];}

        private readonly List<string> _rooms = new List<string>();

        private IInput<string> i_sendToNetwork;

        // Сообщить серверу что клиент отключаeтся.
        private IInput i_sendDestroying;
        private IInput i_dispose;

        void Construction()
        {
            input_to(ref i_sendToNetwork, Header.CLIENT_SEND_NETWORK_EVENT, Send);

            input_to(ref i_sendDestroying, Header.CLIENT_SEND_NETWORK_EVENT, () =>
            {
                try
                {
                    _stream.Write(System.Text.Encoding.UTF8.GetBytes
                        ($"{Server.Client.DISCONNECT}{_.END_MESSAGE}"));
                }
                catch {}

                i_dispose.To();
            });

            input_to(ref i_dispose, Header.WORK_WITCH_OBJECTS_EVENT, Dispose);

            add_event(Header.CLIENT_RECEIVE_NETWORK_EVENT, () =>
            {
                if (_isRunning == false) return;

                try
                {
                    int count = _tcpClient.Available;
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

        void Start()
        {
            SystemInformation
                ("Creating client.", ConsoleColor.Green);

            ReadLine.Start(this);
        }

        void ReadLine.IInformation.Command(string command)
        {
            switch (command)
            {
                case Server.Client.CREATING_ROOM:

                    ConsoleLine("Введите имя для новой комнаты:");
                    string creatingRoomName = System.Console.ReadLine();
                    if (creatingRoomName != "")
                    {
                        i_sendToNetwork.To
                            ($"{Server.Client.CREATING_ROOM}{_.SPLIT_MESSAGE}" +
                                $"{creatingRoomName}{_.END_MESSAGE}");
                    }
                    else SystemInformation
                        ($"Вы не можете создать комнату с пустым именем!",
                            ConsoleColor.Red);

                    break;

                case Server.Client.CONNECTING_TO_ROOM:

                    ConsoleLine("Введите имя комнаты:");
                    string connectingRoomName = System.Console.ReadLine();
                    if (connectingRoomName != "")
                        i_sendToNetwork.To($"{Server.Client.CONNECTING_TO_ROOM}{_.SPLIT_MESSAGE}" +
                            $"{connectingRoomName}{_.END_MESSAGE}");

                    break;

                case Server.Client.DISCONNECTING_ROOM:

                    ConsoleLine("Введите имя комнаты:");
                    string disconnectingRoomName = System.Console.ReadLine();
                    if (disconnectingRoomName != "")
                        i_sendToNetwork.To($"{Server.Client.DISCONNECTING_ROOM}{_.SPLIT_MESSAGE}" +
                            $"{disconnectingRoomName}{_.END_MESSAGE}");

                    break;

                case Server.Client.SEND_MESSAGE_TO_ROOM:

                    ConsoleLine("Имя комнаты:");
                    string sendRoomName = System.Console.ReadLine();
                    if (sendRoomName != "")
                    {
                        ConsoleLine("Введите сообщение:");
                        string sendMessageToRoom = System.Console.ReadLine();
                        if (sendMessageToRoom != "")
                            i_sendToNetwork.To($"{Server.Client.SEND_MESSAGE_TO_ROOM}{_.SPLIT_MESSAGE}" +
                                $"{sendRoomName}{_.SPLIT_MESSAGE}{sendMessageToRoom}{_.END_MESSAGE}");
                    }

                    break;

                case "exit":

                    i_sendToNetwork.To($"{Server.Client.DISCONNECT}{_.END_MESSAGE}");

                    destroy();

                    break;

                default:

                    Console("send - отправить сообщение.");
                    Console($"{Server.Client.CREATING_ROOM} - создать новую комнату.");
                    Console($"{Server.Client.CONNECTING_TO_ROOM} - подключится к сещесвующей комнате.");
                    Console($"{Server.Client.DISCONNECTING_ROOM} - отключится от комнаты.");
                    Console($"{Server.Client.SEND_MESSAGE_TO_ROOM} - отправить сообщение всех клинтам в комнате.");
                    Console($"exit - завершить работу клиeнта.");

                    break;
            }
        }


        void Send(string message)
        {
            if (message == "" || _isRunning == false) return;

            try
            {
                _stream.Write(System.Text.Encoding.UTF8.GetBytes(message));
            }
            catch { destroy(); }
        }

        void Command(byte[] buffer)
        {
            if (buffer.Length == 0) return;

            string[] messages =
                System.Text.Encoding.UTF8.GetString(buffer).Split(_.END_MESSAGE);

            foreach (string m in messages)
            {
                if (m == "") continue;

                string[] commands = m.Split(_.SPLIT_MESSAGE);

                if (commands.Length > 1)
                {
                    switch (commands[0])
                    {
                        case ResponceFromTheServer.SUCCSESS_ROOM_CREATE:

                            _rooms.Add(commands[1]);

                            SystemInformation
                                ($"Комната {commands[1]} успешнa создана!", ConsoleColor.Green);

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_ROOM_CREATE:

                            SystemInformation
                                ($"Комната с именем {commands[1]} уже сущесвует.", ConsoleColor.Red);

                            break;

                        case ResponceFromTheServer.SUCCSESS_CONNECT_TO_ROOM:

                            if (_rooms.Contains(commands[1]))
                            {
                                throw new Exception();
                            }
                            else
                            {
                                _rooms.Add(commands[1]);

                                SystemInformation
                                    ($"Вы подключились к комнате {commands[1]}.", ConsoleColor.Green);
                            }

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_CONNECT_TO_ROOM:

                            if (commands.Length == 3)
                            {
                                SystemInformation
                                    ($"Неудалось подключиться к комнате {commands[1]}.{commands[2]}",
                                        ConsoleColor.Red);
                            }
                            else throw new Exception();


                            break;

                        case ResponceFromTheServer.SUCCSESS_DISCONNECT_ROOM:

                            if (_rooms.Remove(commands[1]))
                            {
                                SystemInformation
                                    ($"Вы успешно отключились от комнаты {commands[1]}",
                                        ConsoleColor.Green);
                            }
                            else throw new Exception();

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_DISCONNECT_ROOM:

                            if (commands.Length == 3)
                            {
                                if (_rooms.Contains(commands[1]))
                                {
                                    // Создатель комнаты начал отключение, 
                                    // до того как пришло наша заявка на отключение.
                                }
                                else SystemInformation
                                    ($"Неудалось отключиться от комнаты {commands[1]}.{commands[2]}.",
                                        ConsoleColor.Red);
                            }
                            else throw new Exception();

                            break;

                        case ResponceFromTheServer.DICONNECT_ROOM:

                            if (commands.Length == 3)
                            {
                                if (_rooms.Remove(commands[1]))
                                {
                                    SystemInformation
                                        ($"Создатель комнаты {commands[2]} уничтожил комнату {commands[1]}." +
                                            $"Вы отключены от комнаты {commands[1]}",
                                                ConsoleColor.Green);
                                }
                            }
                            else throw new Exception();

                            break;

                        case ResponceFromTheServer.RECEIVE_ROOM_MESSAGE:

                            Console("ReceiveFromMessage");

                            if (commands.Length == 4)
                            {
                                if (commands[2] == GetKey())
                                {
                                    SystemInformation
                                        ($"Ваше сообщение \"{commands[3]}\" успешно доставленно всем " +
                                         $" всем клинтам подключоным к комнате {commands[1]}",
                                            ConsoleColor.Green);
                                }
                                else Console($"[Room:{commands[1]}|Client:{commands[2]}] - {commands[3]}");
                            }
                            else throw new Exception();

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_SENDING_MESSAGE_TO_ROOM:

                            if (commands.Length == 4)
                            {
                                SystemInformation
                                    ($"Ваше сообщение {commands[3]} не удалось доствить в комнату {commands[1]}, " +
                                        $"по причине {commands[2]}",
                                            ConsoleColor.Red);
                            }
                            else throw new Exception();

                            break;


                        case ResponceFromTheServer.SUCCSESS_SET_NAME:

                            SystemInformation
                                ($"Вы авторизовались на сервере под именем {commands[1]}.",
                                    ConsoleColor.Green);

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_SET_NAME:

                            SystemInformation
                                ($"На сервере уже присутствует клиент с именем {commands[1]}.",
                                    ConsoleColor.Red);

                            destroy();

                            break;
                    }
                }
            }
        }

        void Configurate()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint
                (IPAddress.Any, System.Convert.ToInt32(Field[LOCAL_PORT_INDEX]));

                _tcpClient = new TcpClient(endPoint);

                _tcpClient.Connect(Field[REMOTE_ADDRESS_INDEX],
                    System.Convert.ToInt32(Field[REMOTE_PORT_INDEX]));

                _stream = _tcpClient.GetStream();

                Send($"{Server.Client.SET_NAME}{_.SPLIT_MESSAGE}{GetKey()}{_.END_MESSAGE}");
            }
            catch (System.Exception ex)
            {
                SystemInformation(ex.ToString(), System.ConsoleColor.Red);
                destroy();
            }
        }

        void Destruction() => _isRunning = false;

        void Stop()
        {
            if (StateInformation.IsCallConfigurate)
                i_sendDestroying.To();

            SystemInformation
                ("Destroying client.", ConsoleColor.Green);

            if (StateInformation.IsCallStart) ReadLine.Stop(this);
        }

        void Dispose()
        {
            if (_tcpClient != null)
            {
                try
                {
                    if (_stream != null)
                    {
                        _stream.Close();
                        _stream.Dispose();
                    }

                    _tcpClient.GetStream().Close();
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                }
                catch { }
            }
        }
    }
}