using System.Net;
using System.Net.Sockets;
using Butterfly;

public struct ResponceFromTheServer
{
    public const string SUCCSESS_SET_NAME = "succsess_set_name";
    public const string UNSUCCSESS_SET_NAME = "unsuccsess_set_name";

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

        void Construction()
        {
            add_event(Header.EVENT_1, () =>
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
                catch (System.Exception ex)
                {
                    _isRunning = false;

                    Destroy($"ListneEvent:{ex}");
                }
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
                case "send":

                    ConsoleLine("Введите сообщение:");
                    string message = System.Console.ReadLine();
                    Send(message);

                    break;

                case Server.Client.CREATING_ROOM:

                    ConsoleLine("Введите имя для новой комнаты:");
                    string roomName = System.Console.ReadLine();
                    if (roomName != "")
                    {
                        Send($"{Server.Client.CREATING_ROOM}{_.SPLIT_MESSAGE}{roomName}{_.END_MESSAGE}");
                    }
                    else SystemInformation
                        ($"Вы не можете создать комнату с пустым именем!", ConsoleColor.Red);

                    break;

                case "exit":

                    Destroy("Exit");

                    break;

                default:
                    Console("send - отправить сообщение.");
                    Console($"{Server.Client.CREATING_ROOM} - создать новую комнату.");
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
            catch (System.Exception ex)
            {
                _isRunning = false;

                Destroy("SEND");
            }
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

                            SystemInformation
                                ($"Комната {commands[1]} успешнa создана!", ConsoleColor.Green);

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_ROOM_CREATE:

                            SystemInformation
                                ($"Комната с именем {commands[1]} уже сущесвует.");

                            break;

                        case ResponceFromTheServer.SUCCSESS_SET_NAME:

                            SystemInformation
                                ($"Вы авторизовались на сервере под именем {commands[1]}.", ConsoleColor.Green);

                            break;

                        case ResponceFromTheServer.UNSUCCSESS_SET_NAME:

                            SystemInformation
                                ($"На сервере уже присутствует клиент с именем {commands[1]}.", ConsoleColor.Red);

                            Destroy($"ListneMessage[{ResponceFromTheServer.UNSUCCSESS_ROOM_CREATE}]");

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

                Send($"{Server.Client.SET_NAME}{_.SPLIT_MESSAGE}HELLO{_.END_MESSAGE}");
            }
            catch (System.Exception ex)
            {
                _isRunning = false;
                SystemInformation(ex.ToString(), System.ConsoleColor.Red);
                Destroy("Configurate");
            }
        }

        void Stop()
        {
            _isRunning = false;

            if (StateInformation.IsCallConfigurate)
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
                    catch
                    { }
                }
            }

            SystemInformation
                ("Destroying client.", ConsoleColor.Green);

            if (StateInformation.IsCallStart) ReadLine.Stop(this);
        }

        private void Destroy(string info)
        {
            Console($"DESTROY:{info}");

            destroy();
        }
    }
}