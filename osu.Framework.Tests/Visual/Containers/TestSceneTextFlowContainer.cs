// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.Containers
{
    public class TestSceneTextFlowContainer : FrameworkTestScene
    {
        [Resolved]
        private FrameworkConfigManager config { get; set; }

        private const string default_text = "Default text\n\nnewline";

        private TextFlowContainer textContainer;

        [SetUpSteps]
        public void Setup()
        {
            AddStep("restore default romanisation settings", () => config.GetBindable<bool>(FrameworkSetting.ShowUnicode).SetDefault());

            AddStep("create text flow", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 300,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White.Opacity(0.1f)
                        },
                        textContainer = new TextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = default_text
                        }
                    }
                };
            });
        }

        [TestCase(Anchor.TopLeft)]
        [TestCase(Anchor.TopCentre)]
        [TestCase(Anchor.TopRight)]
        [TestCase(Anchor.BottomLeft)]
        [TestCase(Anchor.BottomCentre)]
        [TestCase(Anchor.BottomRight)]
        public void TestChangeTextAnchor(Anchor anchor)
        {
            AddStep("change text anchor", () => textContainer.TextAnchor = anchor);
            AddAssert("children have correct anchors", () => textContainer.Children.All(c => c.Anchor == anchor && c.Origin == anchor));
            AddAssert("children are positioned correctly", () =>
            {
                var result = string.Concat(textContainer.Children
                                                        .OrderBy(c => c.ScreenSpaceDrawQuad.TopLeft.Y)
                                                        .ThenBy(c => c is TextFlowContainer.NewLineContainer ? 0 : c.ScreenSpaceDrawQuad.TopLeft.X)
                                                        .Select(c => (c as SpriteText)?.Text.ToString() ?? "\n"));
                return result == default_text;
            });
        }

        [Test]
        public void TestAddTextWithTextAnchor()
        {
            AddStep("change text anchor", () => textContainer.TextAnchor = Anchor.TopCentre);
            AddStep("add text", () => textContainer.AddText("added text"));
            AddAssert("children have correct anchors", () => textContainer.Children.All(c => c.Anchor == Anchor.TopCentre && c.Origin == Anchor.TopCentre));
        }

        [Test]
        public void TestAddLocalisedText()
        {
            const string romanised = "world";
            const string non_romanised = "世界";

            AddStep("enable non-romanised display", () => config.SetValue(FrameworkSetting.ShowUnicode, true));

            AddStep("add normal text", () => textContainer.AddText("hello "));
            AddStep("add romanisable text", () => textContainer.AddText(new RomanisableString(non_romanised, romanised)));

            AddStep("disable non-romanised display", () => config.SetValue(FrameworkSetting.ShowUnicode, false));
        }
    }
}
