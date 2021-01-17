// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Audio.Sample
{
    /// <summary>
    /// A channel playing back an audio sample.
    /// </summary>
    public interface ISampleChannel : IPlayOnlySampleChannel
    {
        /// <summary>
        /// Start a playback of this sample.
        /// </summary>
        /// <param name="restart">Whether to restart the sample from the beginning. If true, any existing playback of the channel will be stopped.</param>
        void Play(bool restart = true);

        void IPlayOnlySampleChannel.Play() => Play();

        /// <summary>
        /// Stop playback and reset position to beginning of sample.
        /// </summary>
        void Stop();

        /// <summary>
        /// States if this sample should repeat.
        /// </summary>
        bool Looping { get; set; }
    }
}
