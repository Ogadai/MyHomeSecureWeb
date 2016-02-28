using MyHomeSecureWeb.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class VideoHub : IDisposable
    {
        public delegate void VideoDataMessage(VideoHubData data);
        public event VideoDataMessage OnData;
        public event Action OnClosed;

        private static Dictionary<string, VideoHub> _instances = new Dictionary<string, VideoHub>();

        public static VideoHub Get(string homeHubId, string node)
        {
            lock (_instances)
            {
                VideoHub videoHub = null;
                var streamId = getStreamId(homeHubId, node);
                if (_instances.ContainsKey(streamId))
                {
                    videoHub = _instances[streamId];
                }
                else {
                    videoHub = new VideoHub(streamId, homeHubId, node);
                    _instances[streamId] = videoHub;
                }

                videoHub.AddRef();
                return videoHub;
            }
        }

        private static string getStreamId(string homeHubId, string node)
        {
            return string.Format("{0}|{1}", homeHubId, node);
        }

        private string _streamId;
        private string _nodeName;
        private int _refCount;
        private ChatHub _chatHub;

        private DateTime _lastRequested = DateTime.MinValue;

        private VideoHub(string streamId, string homeHubId, string nodeName)
        {
            _streamId = streamId;
            _nodeName = nodeName;
            _refCount = 0;
            _chatHub = ChatHub.Get(homeHubId);
        }

        public void ReceivedData(byte[] bytes, int length)
        {
            if (OnData != null)
            {
                OnData(new VideoHubData
                {
                    Bytes = bytes,
                    Length = length
                });
            }
        }
        public void Closed()
        {
            if (OnClosed != null)
            {
                OnClosed();
            }
        }

        private void AddRef()
        {
            _refCount++;
        }

        public void Dispose()
        {
            _refCount--;
            if (_refCount == 0)
            {
                _instances.Remove(_streamId);
                _chatHub.Dispose();
            }
        }
    }

    public class VideoHubWaitable : IDisposable
    {
        private VideoHub _videoHub;
        TaskCompletionSource<VideoHubData> _dataCompletion;

        public VideoHubWaitable(VideoHub videoHub)
        {
            _videoHub = videoHub;
            _dataCompletion = new TaskCompletionSource<VideoHubData>();

            _videoHub.OnData += _videoHub_OnData;
            _videoHub.OnClosed += _videoHub_OnClosed;
        }

        private void _videoHub_OnData(VideoHubData data)
        {
            _dataCompletion.TrySetResult(data);
        }
        private void _videoHub_OnClosed()
        {
            _dataCompletion.TrySetResult(new VideoHubData { Length = 0 });
        }

        public async Task<VideoHubData> WaitData()
        {
            VideoHubData data = await _dataCompletion.Task;
            _dataCompletion = new TaskCompletionSource<VideoHubData>();
            return data;
        }

        public void Dispose()
        {
            _videoHub.OnData -= _videoHub_OnData;
            _videoHub.OnClosed -= _videoHub_OnClosed;
            _videoHub.Dispose();
        }
    }

    public class VideoHubData
    {
        public byte[] Bytes { get; set; }
        public int Length { get; set; }
    }
}
