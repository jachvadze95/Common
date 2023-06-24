using System.Diagnostics;

namespace Common.Performance
{
    public class TimeProfiler : IDisposable
    {
        private readonly Stopwatch _StopWatch;
        private readonly string _operationName;
        public event Action<string>? Log;

        private bool disposed;

        public TimeProfiler(string operationName)
        {
            _StopWatch = Stopwatch.StartNew();
            _operationName = operationName;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            _StopWatch.Stop();
            disposed = true;
            OnLog($"{_operationName} took {_StopWatch.ElapsedMilliseconds} ms");
        }

        protected virtual void OnLog(string message)
        {
            if (Log != null) {
                Log.Invoke(message);
            }
            else Debug.WriteLine(message);
        }

        ~TimeProfiler()
        {
            Dispose();
        }

        public static TimeProfiler Start(string operationName)
        {
            return new TimeProfiler(operationName);
        }
    }
}