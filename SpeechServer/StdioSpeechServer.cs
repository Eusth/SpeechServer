using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeechTransport;

namespace SpeechServer
{
    public class StdioSpeechServer : AbstractSpeechServer
    {
        protected override void Send(ref SpeechResult result)
        {
            Console.WriteLine(result);
        }
    }
}
