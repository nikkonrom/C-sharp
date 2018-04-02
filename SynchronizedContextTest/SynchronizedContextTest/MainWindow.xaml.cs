using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SynchronizedContextTest
{
    public partial class MainWindow
    {
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }


        public static async Task<string> DownloadAsync(WebClient client, Uri uri, CancellationToken token)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            client.DownloadStringCompleted += SetResultAction;

            void SetResultAction(object sender, DownloadStringCompletedEventArgs data)
            {
                if (data.Cancelled)
                {
                    taskCompletionSource.SetCanceled();
                }
                else if (data.Error != null)
                {
                    taskCompletionSource.SetException(data.Error);
                }
                else
                {
                    taskCompletionSource.SetResult(data.Result);
                }
                client.DownloadStringCompleted -= SetResultAction;


            }
            using (token.Register(client.CancelAsync))
            {
                client.DownloadStringAsync(uri);

                return await taskCompletionSource.Task;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = SynchronizationContext.Current;

            var thread = new Thread(DownloadAndTrackAsync);
            thread.Start(context);

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private async void DownloadAndTrackAsync(object state)
        {
            var context = state as SynchronizationContext;
            var client = new WebClient();
            _cancellationTokenSource = new CancellationTokenSource();

            client.DownloadProgressChanged += (sender, args) =>
            {
                var progress = Convert.ToDouble(args.BytesReceived) / Convert.ToDouble(args.TotalBytesToReceive) * 100;
                context?.Post(UpdateProgress, progress);
            };

            var task = DownloadAsync(client, new Uri("http://ftp.byfly.by/test/0mb.txt"), _cancellationTokenSource.Token);
            context?.Post(UpdateStatus, "Downloading");

            try
            {
                Debug.WriteLine(await task);
                context?.Post(UpdateStatus, "Succeeded");
            }
            catch (TaskCanceledException exception)
            {
                Debug.WriteLine(exception.Message);
                context?.Post(UpdateStatus, "Canceled");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                context?.Post(UpdateStatus, "Faulted");
            }
        }

        private void UpdateProgress(object state)
        {
            ProgressBar.Value = (double)state;
        }

        private void UpdateStatus(object state)
        {
            StatusLabel.Content = (string)state;
        }
    }
}
