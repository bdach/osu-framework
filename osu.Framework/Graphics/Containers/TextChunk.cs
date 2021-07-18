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

        public readonly string Text;
        public readonly bool NewLineIsParagraph;
        internal readonly Action<SpriteText> CreationParameters;

        public TextChunk(string text, bool newLineIsParagraph, Action<SpriteText> creationParameters = null)
        {
            Text = text;
            NewLineIsParagraph = newLineIsParagraph;
            CreationParameters = creationParameters;
        }

        public void ApplyParameters(SpriteText spriteText)
        {
            CreationParameters?.Invoke(spriteText);
        }

        public virtual void AppendTo(TextFlowContainer textFlowContainer)
        {
            var parts = CreateSprites(Text, textFlowContainer);
            OnDrawablePartsRecreated(parts);
        }

        protected IEnumerable<Drawable> CreateSprites(string text, TextFlowContainer textFlowContainer)
        {
            bool first = true;
            var sprites = new List<Drawable>();

            foreach (string l in text.Split('\n'))
            {
                if (!first)
                {
                    Drawable lastChild = textFlowContainer.Children.LastOrDefault();

                    if (lastChild != null)
                    {
                        var newLine = new TextFlowContainer.NewLineContainer(NewLineIsParagraph);
                        sprites.Add(newLine);
                        textFlowContainer.Add(newLine, this);
                    }
                }

                foreach (string word in SplitWords(l))
                {
                    if (string.IsNullOrEmpty(word)) continue;

                    var textSprite = textFlowContainer.CreateSpriteTextWithChunk(this);
                    textSprite.Text = word;
                    sprites.Add(textSprite);
                    textFlowContainer.Add(textSprite, this);
                }

                first = false;
            }

            return sprites;
        }

        protected void OnDrawablePartsRecreated(IEnumerable<Drawable> parts) => DrawablePartsRecreated?.Invoke(parts);

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
