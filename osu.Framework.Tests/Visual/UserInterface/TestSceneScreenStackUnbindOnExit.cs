// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;

namespace osu.Framework.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenStackUnbindOnExit : FrameworkTestScene
    {
        [Cached]
        private ScreenStack screenStack = new ScreenStack();

        [Test]
        public void TestScreenExitUnbindDoesNotInterruptLoadComplete()
        {
            AddStep("set up the scenario", () =>
            {
                Child = screenStack;
                screenStack.Push(new Screen());
                screenStack.Push(new BrokenScreen());
            });
            AddUntilStep("wait to get to target screen", () => screenStack.CurrentScreen, Is.InstanceOf<Screen>);
        }

        private partial class BrokenSlider : BasicSliderBar<float>
        {
            [Resolved]
            private ScreenStack screenStack { get; set; } = null!;

            protected override void LoadComplete()
            {
                // exiting the current screen provokes the behaviour of unbinding all bindables in the screen's subtree
                screenStack.CurrentScreen.Exit();

                // ...but the following calls should still take correct effect inside `SliderBar`
                // (namely one consisting of propagating `{Min,Max}Value` into `currentNumberInstantaneous`)
                // so that it doesn't have its internal invariants violated
                CurrentNumber.MinValue = -10;
                CurrentNumber.MaxValue = 10;

                // this notably calls `Scheduler.AddOnce(updateValue)` inside, which will happen *in the imminent future, in the same frame as `LoadComplete()` here.
                // if the above mutations of `{Min,Max}Value` don't correctly propagate inside the slider bar due to an overly eager unbind, this will cause a crash.
                base.LoadComplete();
            }
        }

        private partial class BrokenScreen : Screen
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new BrokenSlider();
            }
        }
    }
}
