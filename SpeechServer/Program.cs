using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Recognition;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using SpeechTransport;

namespace SpeechServer
{
    class Program
    {
        private const string LOCALHOST = "127.0.0.1";
        private const string DEFAULT_LOCALE = "en-US";

        private static string _Locale = DEFAULT_LOCALE;
        private static string[] _Words;
        private static int? _Port;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            ParseArgs(args);


            if (_Words == null && _Words.Length == 0)
            {
                return;
            }

            CultureInfo culture = new CultureInfo(_Locale);
            using (var context = new SpeechRecognitionContext(culture, _Words))
            using (var server = GetSpeechServer())
            {
                server.ListenAsync(context);
                Console.In.ReadToEnd();
            }
        }

        private static void ParseArgs(string[] args)
        {
            for(int i = 0; i < args.Length - 1; i++)
            {
                switch(args[i])
                {
                    case "--words":
                        _Words = args[++i].Split(';');
                        break;
                    case "--locale":
                        _Locale = args[++i];
                        break;
                    case "--port":
                        _Port = int.Parse(args[++i]);
                        break;
                }
            }
        }

        static AbstractSpeechServer GetSpeechServer()
        {
            if (_Port == null)
                return new StdioSpeechServer();
            else
                return new SocketSpeechServer(IPAddress.Parse(LOCALHOST), _Port.Value);
        }

    }
}
