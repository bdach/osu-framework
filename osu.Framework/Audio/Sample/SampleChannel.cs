// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Framework.Audio.Sample
{
    public abstract class SampleChannel : PlayOnlySampleChannel, ISampleChannel
    {
        protected SampleChannel(Sample sample, Action<SampleChannel> onPlay)
            : base(sample, c => onPlay.Invoke((SampleChannel)c))
        {
        }

        public virtual void Play(bool restart = true) => base.Play();

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
                Stop();

            base.Dispose(disposing);
        }
    }
}
