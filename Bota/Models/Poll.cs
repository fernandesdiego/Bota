using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

namespace Bota.Models
{
    public class Poll
    {
        public event EventHandler<PollEventArgs> PollEnded;
        public ulong MessageId { get; set; }

        public string Question { get; set; }

        public string Description { get; set; }

        public Dictionary<ButtonComponent, int> Options { get; set; }

        public List<IUser> Voters { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Duration { get; set; }

        public bool HasEnded { get; set; }

        public ISocketMessageChannel TextChannel { get; set; }

        public Poll(string question, Dictionary<ButtonComponent, int> options, DateTime startTime, int duration, ISocketMessageChannel textChannel, string description = null)
        {
            Question = question;
            Options = options;
            StartTime = startTime;
            Duration = duration;
            TextChannel = textChannel;
            Voters = new List<IUser>();
            Description = description;

            var timer = new Timer();
            timer.Interval = TimeSpan.FromMinutes(Duration).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += OnTimeOut;
            timer.Start();
        }

        private void OnTimeOut(object sender, ElapsedEventArgs e)
        {
            HasEnded = true;
            EndTime = DateTime.Now;
            OnPollEnded(this);
        }

        protected virtual void OnPollEnded(Poll poll)
        {
            PollEnded?.Invoke(this, new PollEventArgs { Poll = poll });
        }
    }

    public class PollEventArgs : EventArgs
    {
        public Poll Poll { get; set; }
    }
}
