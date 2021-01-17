// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Audio.Sample
{
    public interface IPlayOnlySampleChannel : IHasAmplitudes
    {
        /// <summary>
        /// Start a playback of this sample.
        /// </summary>
        void Play();

        /// <summary>
        /// Whether the sample is playing.
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// Whether the sample has finished playback.
        /// </summary>
        bool Played { get; }

        /// <summary>
        /// The length of the underlying sample, in milliseconds.
        /// </summary>
        double Length { get; }
    }
}
