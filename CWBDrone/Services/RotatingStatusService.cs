using CWBDrone.Tools;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWBDrone.Services
{
    public class RotatingStatusService : IService
    {
        protected BaseSocketClient Socket { get; set; }
        protected Queue<string> StatusValues { get; set; }
        protected TimeSpan RotationSpan { get; set; }

        protected Timer rotationTimer;

        public RotatingStatusService(BaseSocketClient socket, Queue<string> statuses = null)
        {
            StatusValues = statuses ?? new Queue<string>();
            Socket = socket;

            rotationTimer = new Timer(async _ =>
            {
                if (StatusValues.Count <= 0) { return; }

                var status = StatusValues.Dequeue();
                if (status.EqualsIgnoreCase(Socket.Activity?.Name ?? "")) { return; }

                StatusValues.Enqueue(status);
                await Socket.SetGameAsync(await VariableFormatting.FormatStatus(Socket, status));
            },
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        public void SetSpeed(int speed) => SetSpeed(TimeSpan.FromSeconds(speed));

        public void SetSpeed(TimeSpan span)
        {
            if (span.TotalSeconds < 30)
            {
                throw new ArgumentOutOfRangeException("span", span, $"The speed must be greater than 30 seconds!");
            }

            RotationSpan = span;
            Rotate();
        }

        public bool Stop() => rotationTimer.Change(Timeout.Infinite, Timeout.Infinite);
        public bool Rotate() => rotationTimer.Change(RotationSpan, RotationSpan);

        public void AddStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) { return; }
            StatusValues.Enqueue(status);
        }

        public void SetStatus(params string[] status)
            => SetStatus(new Queue<string>(status));

        public void SetStatus(IEnumerable<string> status)
            => SetStatus(new Queue<string>(status));

        public void SetStatus(Queue<string> status)
        {
            StatusValues = status ?? throw new ArgumentNullException();
        }

        public string[] GetStatuses() => StatusValues.ToArray();

        public Task<bool> ReadyToDisconnect(CWBDrone bot)
        {
            return Task.FromResult(true);
        }

        public Task Disconnect()
        {
            Stop();
            StatusValues.Clear();
            return Task.CompletedTask;
        }
    }
}
