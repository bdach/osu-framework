﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.Containers
{
    /// <summary>
    /// A <see cref="TextFlowContainer"/> that supports adding icons into its text. Inherit from this class to define reusable custom placeholders for icons.
    /// </summary>
    public class CustomizableTextContainer : TextFlowContainer
    {
        public const string UNESCAPED_LEFT = "[";
        public const string ESCAPED_LEFT = "[[";

        public const string UNESCAPED_RIGHT = "]";
        public const string ESCAPED_RIGHT = "]]";

        public static string Escape(string text) => text.Replace(UNESCAPED_LEFT, ESCAPED_LEFT).Replace(UNESCAPED_RIGHT, ESCAPED_RIGHT);

        public static string Unescape(string text) => text.Replace(ESCAPED_LEFT, UNESCAPED_LEFT).Replace(ESCAPED_RIGHT, UNESCAPED_RIGHT);

        /// <summary>
        /// Sets the placeholders that should be used to replace the numeric placeholders, in the order given.
        /// </summary>
        public IEnumerable<Drawable> Placeholders
        {
            get => placeholders;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                placeholders.Clear();
                placeholders.AddRange(value);
            }
        }

        private readonly List<Drawable> placeholders = new List<Drawable>();
        private readonly Dictionary<string, Delegate> iconFactories = new Dictionary<string, Delegate>();

        /// <summary>
        /// Adds the given drawable as a placeholder that can be used when adding text. The drawable must not have a parent. Returns the index that can be used to reference the added placeholder.
        /// </summary>
        /// <param name="drawable">The drawable to use as a placeholder. This drawable must not have a parent.</param>
        /// <returns>The index that can be used to reference the added placeholder.</returns>
        public int AddPlaceholder(Drawable drawable)
        {
            placeholders.Add(drawable);
            return placeholders.Count - 1;
        }

        /// <summary>
        /// Adds the given factory method as a placeholder. It will be used to create a drawable each time [<paramref name="name"/>] is encountered in the text. The <paramref name="factory"/> method must return a <see cref="Drawable"/> and may contain an arbitrary number of integer parameters. If there are, fe, 2 integer parameters on the factory method, the placeholder in the text would need to look like [<paramref name="name"/>(42, 1337)] supplying the values 42 and 1337 to the method as arguments.
        /// </summary>
        /// <param name="name">The name of the placeholder that the factory should create drawables for.</param>
        /// <param name="factory">The factory method creating drawables.</param>
        protected void AddIconFactory(string name, Delegate factory) => iconFactories.Add(name, factory);

        // I dislike the following overloads as much as you, but if we only had the general overload taking a Delegate, AddIconFactory("test", someInstanceMethod) would not compile (because we would need to cast someInstanceMethod to a delegate type first).
        /// <summary>
        /// Adds the given factory method as a placeholder. It will be used to create a drawable each time [<paramref name="name"/>] is encountered in the text. The <paramref name="factory"/> method must return a <see cref="Drawable"/> and may contain an arbitrary number of integer parameters. If there are, fe, 2 integer parameters on the factory method, the placeholder in the text would need to look like [<paramref name="name"/>(42, 1337)] supplying the values 42 and 1337 to the method as arguments.
        /// </summary>
        /// <param name="name">The name of the placeholder that the factory should create drawables for.</param>
        /// <param name="factory">The factory method creating drawables.</param>
        protected void AddIconFactory(string name, Func<Drawable> factory) => iconFactories.Add(name, factory);

        /// <summary>
        /// Adds the given factory method as a placeholder. It will be used to create a drawable each time [<paramref name="name"/>] is encountered in the text. The <paramref name="factory"/> method must return a <see cref="Drawable"/> and may contain an arbitrary number of integer parameters. If there are, fe, 2 integer parameters on the factory method, the placeholder in the text would need to look like [<paramref name="name"/>(42, 1337)] supplying the values 42 and 1337 to the method as arguments.
        /// </summary>
        /// <param name="name">The name of the placeholder that the factory should create drawables for.</param>
        /// <param name="factory">The factory method creating drawables.</param>
        protected void AddIconFactory(string name, Func<int, Drawable> factory) => iconFactories.Add(name, factory);

        /// <summary>
        /// Adds the given factory method as a placeholder. It will be used to create a drawable each time [<paramref name="name"/>] is encountered in the text. The <paramref name="factory"/> method must return a <see cref="Drawable"/> and may contain an arbitrary number of integer parameters. If there are, fe, 2 integer parameters on the factory method, the placeholder in the text would need to look like [<paramref name="name"/>(42, 1337)] supplying the values 42 and 1337 to the method as arguments.
        /// </summary>
        /// <param name="name">The name of the placeholder that the factory should create drawables for.</param>
        /// <param name="factory">The factory method creating drawables.</param>
        protected void AddIconFactory(string name, Func<int, int, Drawable> factory) => iconFactories.Add(name, factory);

        internal bool TryGetIconFactory(string name, out Delegate iconFactory) => iconFactories.TryGetValue(name, out iconFactory);

        internal override TextChunk CreateChunkFor(string text, bool newLineIsParagraph, Action<SpriteText> creationParameters = null) =>
            new CustomizableTextChunk(text, newLineIsParagraph, creationParameters);
    }
}
