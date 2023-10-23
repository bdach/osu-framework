// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Tests.Visual;

namespace osu.Framework.Tests.Audio
{
    [HeadlessTest]
    public partial class DrawableAudioWrapperTest : FrameworkTestScene
    {
        [Test]
        public void TestAdjustmentsPreservedOnOwnedComponentAfterRemoval()
        {
            TrackVirtual track = null!;
            SlowDisposingDrawableAudioWrapper wrapper = null!;

            AddStep("add slow disposing wrapper for owned track",
                () => Child = wrapper = new SlowDisposingDrawableAudioWrapper(track = new TrackVirtual(1000)));
            AddStep("mute wrapper", () => wrapper.AddAdjustment(AdjustableProperty.Volume, new BindableDouble()));
            AddStep("expire wrapper", () => wrapper.Expire());
            AddAssert("component still muted", () => track.AggregateVolume.Value, () => Is.EqualTo(0));
            AddStep("allow disposal to complete", () => wrapper.AllowDisposal.Set());
        }

        [Test]
        public void TestAdjustmentsRevertedOnOwnedComponentAfterRemoval()
        {
            TrackVirtual track = null!;
            SlowDisposingDrawableAudioWrapper wrapper = null!;

            AddStep("add slow disposing wrapper for non-owned track",
                () => Child = wrapper = new SlowDisposingDrawableAudioWrapper(track = new TrackVirtual(1000), false));
            AddStep("mute wrapper", () => wrapper.AddAdjustment(AdjustableProperty.Volume, new BindableDouble()));
            AddStep("expire wrapper", () => wrapper.Expire());
            AddAssert("component unmuted", () => track.AggregateVolume.Value, () => Is.EqualTo(1));
            AddStep("allow disposal to complete", () => wrapper.AllowDisposal.Set());
        }

        /// <summary>
        /// When changing a <see cref="DrawableAudioWrapper"/>'s parent, its adjustments are expected to transfer immediately.
        /// This is because in some scnenarios (i.e. sample pooling), it can be the case that the component requesting sample playback
        /// does so before the <see cref="DrawableAudioWrapper"/> has a chance to update itself.
        /// </summary>
        [Test]
        public void TestAdjustmentsAppliedImmediatelyOnParentChange()
        {
            AudioContainer first = new AudioContainer();
            AudioContainer second = new AudioContainer();
            SlowDisposingDrawableAudioWrapper wrapper = new SlowDisposingDrawableAudioWrapper(new TrackVirtual(1000));

            AddStep("load containers", () =>
            {
                LoadComponent(first);
                LoadComponent(second);
            });

            AddStep("set volume on first audio container", () => first.Volume.Value = 0.8);
            AddStep("set volume on second audio container", () => second.Volume.Value = 0.5);

            AddStep("add wrapper to first container", () => first.Add(wrapper));
            AddAssert("wrapper has correct aggregate volume", () => wrapper.AggregateVolume.Value, () => Is.EqualTo(0.8));
            AddStep("update containers", () =>
            {
                first.UpdateSubTree();
                second.UpdateSubTree();
            });
            AddAssert("wrapper has correct aggregate volume", () => wrapper.AggregateVolume.Value, () => Is.EqualTo(0.8));

            AddStep("transfer wrapper to second container", () =>
            {
                first.Remove(wrapper, false);
                second.Add(wrapper);
            });
            AddAssert("wrapper has correct aggregate volume", () => wrapper.AggregateVolume.Value, () => Is.EqualTo(0.5));
            AddStep("update containers", () =>
            {
                first.UpdateSubTree();
                second.UpdateSubTree();
            });
            AddAssert("wrapper has correct aggregate volume", () => wrapper.AggregateVolume.Value, () => Is.EqualTo(0.5));

            AddStep("allow disposal to complete", () => wrapper.AllowDisposal.Set());
        }

        private partial class SlowDisposingDrawableAudioWrapper : DrawableAudioWrapper
        {
            public ManualResetEvent AllowDisposal { get; private set; } = new ManualResetEvent(false);

            public SlowDisposingDrawableAudioWrapper(IAdjustableAudioComponent component, bool disposeUnderlyingComponentOnDispose = true)
                : base(component, disposeUnderlyingComponentOnDispose)
            {
            }

            protected override void Dispose(bool isDisposing)
            {
                AllowDisposal.WaitOne(10_000);

                base.Dispose(isDisposing);
            }
        }
    }
}
