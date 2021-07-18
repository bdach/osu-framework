// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.Containers
{
    public class TextPartManual : ITextPart
    {
        event Action<IEnumerable<Drawable>> ITextPart.DrawablePartsRecreated
        {
            add { }
            remove { }
        }

        event Action ITextPart.ContentChanged
        {
            add { }
            remove { }
        }

        public readonly SpriteText SpriteText;

        public TextPartManual(SpriteText spriteText)
        {
            SpriteText = spriteText;
        }

        public IEnumerable<Drawable> CreateDrawablesFor(TextFlowContainer textFlowContainer) => SpriteText.Yield();
    }
}
