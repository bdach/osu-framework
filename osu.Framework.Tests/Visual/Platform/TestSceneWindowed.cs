// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Framework.Tests.Visual.Platform
{
    public class TestSceneWindowed : FrameworkTestScene
    {
        private readonly Bindable<WindowMode> windowMode = new Bindable<WindowMode>();
        private readonly Bindable<Size> windowSize = new BindableSize();

        private DesktopGameWindow gameWindow;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, GameHost host)
        {
            gameWindow = host.Window as DesktopGameWindow;

            config.BindWith(FrameworkSetting.WindowMode, windowMode);
            config.BindWith(FrameworkSetting.WindowedSize, windowSize);
        }

        [TestCase(640, 480)]
        [TestCase(2560, 1080)]
        public void TestWindowedSizeChange(int width, int height)
        {
            var size = new Size(width, height);
            AddStep("set windowed mode", () => windowMode.Value = WindowMode.Windowed);

            AddStep($"set windowed size to {width}x{height}", () => windowSize.Value = size);

            AddAssert("window size changed", () => gameWindow.ClientSize == size);
        }
    }
}
