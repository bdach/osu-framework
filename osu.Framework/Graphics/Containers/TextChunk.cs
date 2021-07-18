// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.Containers
{
    internal class TextChunk : ITextPart
    {
        public event Action<IEnumerable<Drawable>> DrawablePartsRecreated;

        private readonly string text;
        private readonly bool newLineIsParagraph;
        private readonly Action<SpriteText> creationParameters;

        public TextChunk(string text, bool newLineIsParagraph, Action<SpriteText> creationParameters = null)
        {
            this.text = text;
            this.newLineIsParagraph = newLineIsParagraph;
            this.creationParameters = creationParameters;
        }

        public void ApplyParameters(SpriteText spriteText)
        {
            creationParameters?.Invoke(spriteText);
        }

        public IEnumerable<Drawable> CreateDrawablesFor(TextFlowContainer textFlowContainer)
        {
            // !newLineIsParagraph effectively means that we want to add just *one* paragraph, which means we need to make sure that any previous paragraphs
            // are terminated. Thus, we add a NewLineContainer that indicates the end of the paragraph before adding our current paragraph.
            if (!newLineIsParagraph)
            {
                var newLine = new TextNewLine(true);
                textFlowContainer.AddPart(newLine);
            }

            var parts = CreateSprites(text, textFlowContainer);
            DrawablePartsRecreated?.Invoke(parts);
            return parts;
        }

        protected virtual IEnumerable<Drawable> CreateSprites(string text, TextFlowContainer textFlowContainer)
        {
            bool first = true;
            var sprites = new List<Drawable>();

            foreach (string l in text.Split('\n'))
            {
                if (!first)
                {
                    Drawable lastChild = sprites.LastOrDefault() ?? textFlowContainer.Children.LastOrDefault();

                    if (lastChild != null)
                    {
                        var newLine = new TextFlowContainer.NewLineContainer(newLineIsParagraph);
                        sprites.Add(newLine);
                    }
                }

                foreach (string word in SplitWords(l))
                {
                    if (string.IsNullOrEmpty(word)) continue;

                    var textSprite = textFlowContainer.CreateSpriteTextWithChunk(this);
                    textSprite.Text = word;
                    sprites.Add(textSprite);
                }

                first = false;
            }

            return sprites;
        }

        protected string[] SplitWords(string text)
        {
            var words = new List<string>();
            var builder = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                if (i == 0 || char.IsSeparator(text[i - 1]) || char.IsControl(text[i - 1]))
                {
                    words.Add(builder.ToString());
                    builder.Clear();
                }

                builder.Append(text[i]);
            }

            if (builder.Length > 0)
                words.Add(builder.ToString());

            return words.ToArray();
        }
    }
}
