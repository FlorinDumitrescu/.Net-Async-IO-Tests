using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelUrlRetrieve
{
    class Tester
    {
        #region constants
        private const string URL =
            //"http://msdn.microsoft.com/en-us/";
            //"http://localhost/StaticIISSite/test.txt";
            //"http://localhost:50001/api/LongRunning";
            "http://localhost:49970/api/LongRunning";
        private const int NUMBER_OF_REQUESTS = 5000;
            // 1000000;
        private const int MAX_CONCURENT_REQUESTS = 1500;
        private const int DELAY_TIME = 50; // miliseconds
        private const int DEFAULT_CONNECTION_LIMIT = 500;
        private const bool ENABLE_CPU_BOUND_OPERATION = false;
        #endregion constants

        #region private fields
        private readonly object _syncLock = new object();
        private volatile int _itemsLeft;
        private volatile int _activeRequestsCount;
        private int _successfulCalls;
        private int _failedCalls;
        private DateTime _utcStartTime;
        private DateTime _utcEndTime;
        #endregion private fields

        #region private methods
        private void CpuBoundedOperation()
        {
            StringBuilder sb = new StringBuilder();
            string result = null;

            for (int i = 0; i < 5000; i++)
            {
                string temp = "The ".Replace(" ", " quick ")
                    .Replace("quick ", "quick brown ")
                    .Replace("brown ", "brown fox ")
                    .Replace("fox ", "fox jumps ")
                    .Replace("jumps ", "jumps over ")
                    .Replace("over ", "over the lazy ")
                    .Replace("lazy ", "lazy dog.");

                sb.Append(temp);
            }

            result = sb.ToString();
        }

        private void TestInit()
        {
            _itemsLeft = NUMBER_OF_REQUESTS;
            _successfulCalls = 0;
            _failedCalls = 0;
            _utcStartTime = DateTime.UtcNow;
            _activeRequestsCount = 0;
            ServicePointManager.DefaultConnectionLimit = DEFAULT_CONNECTION_LIMIT;
        }

        private void DisplayTestResults()
        {
            TimeSpan interval = _utcEndTime - _utcStartTime;
            Console.WriteLine("Operation finished in " + interval.ToString("c"));
            Console.WriteLine("Calls succeeded: {0}; calls failed: {1}", _successfulCalls, _failedCalls);
        }

        private async void ProcessUrlAsyncWithReqCount(HttpClient httpClient)
        {
            try
            {
                Interlocked.Increment(ref _activeRequestsCount);
                Task<byte[]> getBytesAtUrlTask = httpClient.GetByteArrayAsync(URL);

                byte[] callResult = await getBytesAtUrlTask;

                Interlocked.Increment(ref _successfulCalls);
                Interlocked.Decrement(ref _activeRequestsCount);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCalls);
                Interlocked.Decrement(ref _activeRequestsCount);
            }

            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }

        private async void ProcessUrlAsync(HttpClient httpClient)
        {
            HttpResponseMessage httpResponse = null;
            if (ENABLE_CPU_BOUND_OPERATION)
                this.CpuBoundedOperation();

            try
            {
                //Task<HttpResponseMessage> getTask = httpClient.GetAsync(URL);
                httpResponse = await httpClient.GetAsync(URL);

                Interlocked.Increment(ref _successfulCalls);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCalls);
            }
            finally
            { 
                if(httpResponse != null) httpResponse.Dispose();
            }

            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }

        private void PerformWebRequestGet()
        { 
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            if (ENABLE_CPU_BOUND_OPERATION)
                this.CpuBoundedOperation();

            try
            {
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                request.KeepAlive = true;
                response = (HttpWebResponse)request.GetResponse();
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        private async void PerformWebRequestGetAsync()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                request.KeepAlive = true;
                response = (HttpWebResponse)await request.GetResponseAsync();

                Interlocked.Increment(ref _successfulCalls);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCalls);
            }
            finally
            {
                if (response != null) response.Close();
            }

            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }

        private async void PerformWebRequestGetAsyncWithReqCount()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                Interlocked.Increment(ref _activeRequestsCount);

                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                request.KeepAlive = true;
                response = (HttpWebResponse)await request.GetResponseAsync();

                Interlocked.Increment(ref _successfulCalls);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCalls);
            }
            finally
            {
                Interlocked.Decrement(ref _activeRequestsCount);
                if (response != null) response.Close();
            }

            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }
        #endregion private methods

        #region public methods
        public void TestParallel()
        {
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            byte[] result = httpClient.GetByteArrayAsync(URL).Result;
                        }

                        Interlocked.Increment(ref _successfulCalls);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _failedCalls);
                    }

                    lock (_syncLock)
                    {
                        _itemsLeft--;
                        if (_itemsLeft == 0)
                        {
                            _utcEndTime = DateTime.UtcNow;
                            this.DisplayTestResults();
                        }
                    }
                });
            }
        }

        public void TestParallel2()
        {
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.PerformWebRequestGet();
                        Interlocked.Increment(ref _successfulCalls);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _failedCalls);
                    }

                    lock (_syncLock)
                    {
                        _itemsLeft--;
                        if (_itemsLeft == 0)
                        {
                            _utcEndTime = DateTime.UtcNow;
                            this.DisplayTestResults();
                        }
                    }
                });
            }
        }

        public void TestParallel3()
        {
            this.TestInit();
            LimitedConcurencyWorkerPool workQueue = new LimitedConcurencyWorkerPool(500, null);

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                workQueue.QueueWorkItem(() =>
                {
                    try
                    {
                        this.PerformWebRequestGet();
                        Interlocked.Increment(ref _successfulCalls);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _failedCalls);
                    }

                    lock (_syncLock)
                    {
                        _itemsLeft--;
                        if (_itemsLeft == 0)
                        {
                            _utcEndTime = DateTime.UtcNow;
                            this.DisplayTestResults();
                        }
                    }
                });
            }
        }

        public void TestSynchronous2()
        {
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                try
                {
                    this.PerformWebRequestGet();
                    _successfulCalls++;
                }
                catch (Exception ex)
                {
                    _failedCalls++;
                }

                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }

        public void TestSynchronous()
        { 
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    byte[] result = httpClient.GetByteArrayAsync(URL).Result;

                    _successfulCalls++;
                }
                catch (Exception ex)
                {
                    _failedCalls++;
                }

                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    _utcEndTime = DateTime.UtcNow;
                    this.DisplayTestResults();
                }
            }
        }

        public async void TestAsyncWithDelay()
        {
            this.TestInit();
            HttpClient httpClient = new HttpClient();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                if (_activeRequestsCount >= MAX_CONCURENT_REQUESTS)
                    await Task.Delay(DELAY_TIME);

                ProcessUrlAsyncWithReqCount(httpClient);
            }
        }

        public async void TestAsync()
        {
            this.TestInit();
            HttpClient httpClient = new HttpClient();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                ProcessUrlAsync(httpClient);
            }
        }

        public async void TestAsync2()
        {
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                PerformWebRequestGetAsync();
            }
        }

        public async void TestAsyncWithDelay2()
        {
            this.TestInit();

            for (int i = 0; i < NUMBER_OF_REQUESTS; i++)
            {
                if (_activeRequestsCount >= MAX_CONCURENT_REQUESTS)
                    await Task.Delay(DELAY_TIME);

                PerformWebRequestGetAsyncWithReqCount();
            }
        }
        #endregion public methods
    }
}
