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

        private readonly SpriteText spriteText;

        public TextPartManual(SpriteText spriteText)
        {
            this.spriteText = spriteText;
        }

        public IEnumerable<Drawable> CreateDrawablesFor(TextFlowContainer textFlowContainer) => spriteText.Yield();
    }
}
