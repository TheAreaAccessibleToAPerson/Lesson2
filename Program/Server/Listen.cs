using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Server
{
    public sealed class Listen : Controller.Board.LocalField<string[]>.Output<TcpClient>
    {
        TcpListener _listener;

        private IPEndPoint _localPoint;

        private bool _isRunning = true;

        private const int DATA_COUNT = 2; // Количесво данных.
        private const int ADDRESS_INDEX = 0;
        private const int PORT_INDEX = 1;

        private IInput<string> _inputToAddListenName;
        private IInput<string> _inputToRemoveListenName;

        void Construction()
        {
            send_message(ref _inputToAddListenName, Server.Main.ADD_LISTEN_OBJECT);
            send_message(ref _inputToRemoveListenName, Server.Main.REMOVE_LISTEN_OBJECT);

            add_thread(GetKey(), Accept, 0, Thread.Priority.Normal);
        }

        void Start()
        {
            SystemInformation
                ($"Start listen clients [Address:{Field[ADDRESS_INDEX]}, Port:{Field[PORT_INDEX]}]");

            _inputToAddListenName.To(GetKey());

            try
            {
                _listener.Start();
            }
            catch (System.Exception ex)
            {
                SystemInformation(ex.ToString(), ConsoleColor.Red);
                destroy();
            }
        }

        void Accept()
        {
            if (_isRunning == false) return;
            {
                try
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();

                        output(client);
                    }
                }
                catch (System.Exception ex)
                {
                    SystemInformation(ex.ToString(), ConsoleColor.Red);

                    _isRunning = false;

                    destroy();
                }
            }
        }

        void Stop()
        {
            _isRunning = false;

            if (StateInformation.IsCallStart)
            {
                _inputToRemoveListenName.To(GetKey());
            }

            try
            {
                _listener.Stop();
            }
            finally
            {

            }

            SystemInformation
                ($"Stop listen clients [Address:{Field[ADDRESS_INDEX]}, Port:{Field[PORT_INDEX]}]");
        }

        void Configurate()
        {
            if (Field.Length > DATA_COUNT)
            {
                SystemInformation($"Вы передали неверное количесво данных." +
                    $"Ожидалось {DATA_COUNT}, но поступило {Field.Length}.");

                destroy();

                return;
            }

            try
            {
                _localPoint = new IPEndPoint
                    (IPAddress.Parse(Field[ADDRESS_INDEX]), Convert.ToInt32(Field[PORT_INDEX]));

                _listener = new TcpListener(_localPoint);
            }
            catch
            {
                destroy();
            }
        }
    }
}