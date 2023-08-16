using System.Net.Sockets;
using Butterfly;

namespace Server
{
    public sealed class Main : Controller.Board, ReadLine.IInformation
    {
        public const string ADD_LISTEN_OBJECT = "AddListenObject";
        public const string REMOVE_LISTEN_OBJECT = "RemoveListenObject";

        private readonly List<string> _listenClientsName = new List<string>();

        private IInput<string> i_inputToDestroyingListenClientsIndex;
        private IInput i_inputToShowAllListenClientsName;

        void Construction()
        {
            obj<ClientsManager>("ClientsManager");

            listen_message<string>(ADD_LISTEN_OBJECT)
                .output_to(_listenClientsName.Add);

            listen_message<string>(REMOVE_LISTEN_OBJECT)
                .output_to((value) =>
                {
                    _listenClientsName.Remove(value);
                });

            input_to(ref i_inputToShowAllListenClientsName, Header.WORK_WITCH_OBJECTS_EVENT, () =>
            {
                int count = _listenClientsName.Count;

                if (count == 0)
                {
                    SystemInformation("В данный момент нету не одного работающего ListenClients.",
                        ConsoleColor.Green);
                }

                string str = "\n";
                for (int i = 0; i < count; i++)
                {
                    if (try_obj(_listenClientsName[i], out Server.Listen listenObject))
                    {
                        str += $"{i + 1})[State:{listenObject.StateInformation.GetString()}]{_listenClientsName[i]}\n";
                    }
                }

                Console(str);
            });

            input_to(ref i_inputToDestroyingListenClientsIndex, Header.WORK_WITCH_OBJECTS_EVENT, (name) =>
            {
                if (try_obj(name, out Server.Listen listenObject))
                {
                    listenObject.destroy();
                }
            });
        }

        void Start()
        {
            SystemInformation("Server start.", ConsoleColor.Green);

            ReadLine.Start(this);
        }

        void ReadLine.IInformation.Command(string command)
        {
            switch (command)
            {
                case "creating_listen_clients":

                    ConsoleLine("Введи адресс:");
                    string creatingListenClientsAddress = System.Console.ReadLine();

                    ConsoleLine("Введите порт:");
                    string creatingListenClientsPort = System.Console.ReadLine();

                    string creatingListenClientsName
                        = $"{creatingListenClientsAddress}/{creatingListenClientsPort}";

                    if (try_obj(creatingListenClientsName, out Server.Listen listenObject))
                    {
                        ConsoleLine("Прослушка клиeнтов уже осуществляется по данному адресу и порту " +
                            " желаете пересоздать ее? yes/no:");

                        string isDestroyListenClients = System.Console.ReadLine();

                        if (isDestroyListenClients == "yes")
                        {
                            listenObject.destroy();
                        }
                        else return;
                    }

                    obj<Listen, TcpClient>(creatingListenClientsName,
                        new string[]
                        {
                            creatingListenClientsAddress,
                            creatingListenClientsPort
                        })
                        .send_message_to(ClientsManager.ADD_CLIENT);

                    break;

                case "destroying_listen_clients":

                    if (_listenClientsName.Count == 0)
                    {
                        SystemInformation("В данный момент нету неодного запущеного ListenClients.");
                    }
                    else
                    {
                        SystemInformation("wait, loading all listener...");
                        i_inputToShowAllListenClientsName.To();

                        string index = System.Console.ReadLine();
                        i_inputToDestroyingListenClientsIndex.To(index);
                    }

                    break;


                case "exit":

                    destroy();

                    break;

                default:

                    Console("creating_listen_clients - создать прослушивание клинтов.");
                    Console("destroying_listen_clients - уничтожить прослушку клинтов.");
                    Console("exit - остановить работу сервера.");

                    break;
            }
        }

        void Stop()
        {
            SystemInformation("Stop server.", ConsoleColor.Green);

            if (StateInformation.IsCallStart) ReadLine.Stop(this);
        }
    }
}