using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoLibrary;

namespace YouTubeVideoDownload.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string videoUrl;
        private string downloadStatus;
        private Process goodbyeDpiProcess;

        public string VideoUrl
        {
            get => videoUrl;
            set
            {
                videoUrl = value;
                OnPropertyChanged(nameof(VideoUrl));
                DownloadVideoCommand.RaiseCanExecuteChanged();
            }
        }

        public string DownloadStatus
        {
            get => downloadStatus;
            set
            {
                downloadStatus = value;
                OnPropertyChanged(nameof(DownloadStatus));
            }
        }

        public RelayCommand DownloadVideoCommand { get; }
        public RelayCommand StartGoodbyeDpiCommand { get; }
        public RelayCommand StopGoodbyeDpiCommand { get; }

        public MainViewModel()
        {
            DownloadVideoCommand = new RelayCommand(ExecuteDownloadVideo, CanDownloadVideo);
            StartGoodbyeDpiCommand = new RelayCommand(ExecuteStartGoodbyeDpi, CanStartGoodbyeDpi);
            StopGoodbyeDpiCommand = new RelayCommand(ExecuteStopGoodbyeDpi, CanStopGoodbyeDpi);
        }

        private async void ExecuteDownloadVideo()
        {
            if (string.IsNullOrWhiteSpace(VideoUrl))
            {
                DownloadStatus = "Введите URL видео.";
                return;
            }

            try
            {
                var youtube = YouTube.Default;
                var video = await Task.Run(() => youtube.GetVideo(VideoUrl));
                var filePath = Path.Combine("C:\\Users\\kudry\\Downloads", video.FullName);

                await File.WriteAllBytesAsync(filePath, video.GetBytes());
                DownloadStatus = $"Видео скачано: {filePath}";
            }
            catch (Exception ex)
            {
                DownloadStatus = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanDownloadVideo()
        {
            return !string.IsNullOrWhiteSpace(VideoUrl);
        }

        private void ExecuteStartGoodbyeDpi()
        {
            try
            {
                goodbyeDpiProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine("C:\\MyProject\\YouTubeVideoDownload\\YouTubeVideoDownload\\bin\\Debug\\net8.0-windows\\goodbyedpi.exe"),
                        Arguments = "",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                goodbyeDpiProcess.Start();
                DownloadStatus = "GoodbyeDPI запущен.";
            }
            catch (Exception ex)
            {
                DownloadStatus = $"Ошибка запуска GoodbyeDPI: {ex.Message}";
            }
        }

        private bool CanStartGoodbyeDpi()
        {
            return !IsGoodbyeDpiRunning();
        }

        private void ExecuteStopGoodbyeDpi()
        {
            if (goodbyeDpiProcess != null && !goodbyeDpiProcess.HasExited)
            {
                goodbyeDpiProcess.Kill();
                goodbyeDpiProcess.Dispose();
                goodbyeDpiProcess = null;
                DownloadStatus = "GoodbyeDPI остановлен.";
            }
        }

        private bool CanStopGoodbyeDpi()
        {
            return IsGoodbyeDpiRunning();
        }

        private bool IsGoodbyeDpiRunning()
        {
            return goodbyeDpiProcess != null && !goodbyeDpiProcess.HasExited;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

