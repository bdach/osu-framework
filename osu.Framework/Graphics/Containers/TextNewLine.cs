// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Framework.Graphics.Containers
{
    public class TextNewLine : ITextPart
    {
        public event Action<IEnumerable<Drawable>> DrawablePartsRecreated;

        public readonly bool IndicatesNewParagraph;

        private TextFlowContainer.NewLineContainer newLineContainer;

        public TextNewLine(bool indicatesNewParagraph)
        {
            IndicatesNewParagraph = indicatesNewParagraph;
        }

        public void AppendTo(TextFlowContainer textFlowContainer)
        {
            textFlowContainer.Add(newLineContainer = new TextFlowContainer.NewLineContainer(IndicatesNewParagraph), this);
            DrawablePartsRecreated?.Invoke(newLineContainer.Yield());
        }
    }
}
