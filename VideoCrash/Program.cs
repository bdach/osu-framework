// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Video;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace VideoCrash
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (GameHost host = new HeadlessGameHost(@"sample-game"))
            using (Game game = new SampleGameGame())
                host.Run(game);
        }
    }

    public partial class SampleGameGame : Game
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(SampleGameGame).Assembly), "Resources"));
            var store = new NamespacedResourceStore<byte[]>(Resources, @"Videos");

            Add(new Video(store.GetStream(@"h264.avi"))
            {
                RelativeSizeAxes = Axes.Both
            });

            Scheduler.AddDelayed(Exit, 5000);
        }
    }
}
