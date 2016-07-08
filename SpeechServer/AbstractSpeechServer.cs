using SpeechTransport;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Speech.Recognition;

namespace SpeechServer
{
    public abstract class AbstractSpeechServer : IDisposable
    {
       
        private object _Lock = new object();
        private bool _IsListening = false;
        private SpeechRecognitionContext _CurrentContext;
        private ConcurrentQueue<SpeechResult> _Queue = new ConcurrentQueue<SpeechResult>();

        private int _IdCounter = 0;

        protected abstract void Send(ref SpeechResult result);
        

        public void Listen(SpeechRecognitionContext context)
        {
            // Only one listener at a time!
            if (_IsListening) return;

            _IsListening = true;
            LoadContext(context);

            while (_IsListening)
            {
                lock (_Lock)
                {
                    Monitor.Wait(_Lock);
                }

                while (_IsListening && !_Queue.IsEmpty)
                {
                    SpeechResult payload;
                    while (_Queue.TryDequeue(out payload))
                    {
                        Send(ref payload);
                    }
                }
            }
        }

        public async Task ListenAsync(SpeechRecognitionContext context)
        {
            await Task.Factory.StartNew(delegate
            {
                Listen(context);
            });
        }

        public void Stop()
        {
            _IsListening = false;
            UnloadContext();

            // Wake up for the listener to die
            lock (_Lock)
            {
                Monitor.PulseAll(_Lock);
            }
        }

        public virtual void Dispose()
        {
            UnloadContext();
        }

        private void LoadContext(SpeechRecognitionContext context)
        {
            _CurrentContext = context;

            _CurrentContext.Engine.SpeechRecognized += OnSpeechRecognized;
            _CurrentContext.Engine.SpeechHypothesized += OnSpeechHypothesized;

            //_CurrentContext.Engine.Enabled = true;
            _CurrentContext.Engine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void UnloadContext()
        {
            if (_CurrentContext == null) return;


            _CurrentContext.Engine.SpeechRecognized -= OnSpeechRecognized;
            _CurrentContext.Engine.SpeechHypothesized -= OnSpeechHypothesized;

            //_CurrentContext.Engine.Enabled = false;
            _CurrentContext.Engine.RecognizeAsyncStop();

            _CurrentContext = null;
        }

        void OnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            if (e.Result.Grammar.Name != "random")
            {
                _Queue.Enqueue(
                    new SpeechResult()
                    {
                        Text = e.Result.Text,
                        Confidence = e.Result.Confidence,
                        Final = false,
                        ID = ++_IdCounter
                    }
                );

                lock (_Lock)
                {
                    Monitor.PulseAll(_Lock);
                }
            }
        }

        void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Grammar.Name != "random")
            {
                _Queue.Enqueue(
                    new SpeechResult()
                    {
                        Text = e.Result.Text,
                        Confidence = e.Result.Confidence,
                        Final = true,
                        ID = _IdCounter
                    }
                );

                lock (_Lock)
                {
                    Monitor.PulseAll(_Lock);
                }
            }
        }
    }
}
