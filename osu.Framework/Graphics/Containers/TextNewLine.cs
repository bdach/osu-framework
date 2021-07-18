// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Framework.Graphics.Containers
{
    public class TextNewLine : ITextPart
    {
        event Action ITextPart.ContentChanged
        {
            add { }
            remove { }
        }

        public event Action<IEnumerable<Drawable>> DrawablePartsRecreated;

        public readonly bool IndicatesNewParagraph;

        public TextNewLine(bool indicatesNewParagraph)
        {
            IndicatesNewParagraph = indicatesNewParagraph;
        }

        public IEnumerable<Drawable> CreateDrawablesFor(TextFlowContainer textFlowContainer)
        {
            var drawables = new[] { new TextFlowContainer.NewLineContainer(IndicatesNewParagraph) };
            DrawablePartsRecreated?.Invoke(drawables);
            return drawables;
        }
    }
}
