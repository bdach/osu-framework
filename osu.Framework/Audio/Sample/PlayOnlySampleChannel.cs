// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Statistics;

namespace osu.Framework.Audio.Sample
{
    public abstract class PlayOnlySampleChannel : AdjustableAudioComponent, IPlayOnlySampleChannel
    {
        protected bool WasStarted;

        protected Sample Sample { get; }

        protected readonly Action<PlayOnlySampleChannel> OnPlay;

        protected PlayOnlySampleChannel(Sample sample, Action<PlayOnlySampleChannel> onPlay)
        {
            Sample = sample ?? throw new ArgumentNullException(nameof(sample));
            OnPlay = onPlay;
        }

        public virtual void Play()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not play disposed samples.");

            OnPlay(this);
            WasStarted = true;
        }

        public virtual void Stop()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not stop disposed samples.");
        }

        protected override void UpdateState()
        {
            FrameStatistics.Increment(StatisticsCounterType.SChannels);
            base.UpdateState();
        }

        public abstract bool Playing { get; }

        public virtual bool Played => WasStarted && !Playing;

        public double Length => Sample.Length;

        public override bool IsAlive => base.IsAlive && !Played;

        public virtual ChannelAmplitudes CurrentAmplitudes { get; } = ChannelAmplitudes.Empty;
    }
}
