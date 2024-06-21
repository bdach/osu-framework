// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Framework.Timing;

namespace osu.Framework.Graphics.Containers
{
    /// <summary>
    /// This is a variant of <see cref="DelayedLoadUnloadWrapper"/> geared specifically for use with <see cref="PoolableDrawable"/> and <see cref="DrawablePool{T}"/>,
    /// which leverages several simplifications and differences in usage to standard drawables, such as:
    /// <list type="bullet">
    /// <item>not needing to handle async load</item>
    /// <item>not needlessly taking out a poolable if the delayed load is interrupted</item>
    /// <item>not disposing <see cref="Content"/> on unload (returning it to pool instead)</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Any adjustments to <see cref="DelayedLoadUnloadWrapper"/> logic should be cross-checked against this class too.
    /// </remarks>
    public partial class PoolableLoadUnloadWrapper<T> : CompositeDrawable
        where T : PoolableDrawable, new()
    {
        [Resolved]
        protected Game Game { get; private set; } = null!;

        private T? content;

        public T? Content
        {
            get => content;
            protected set
            {
                if (content == value)
                    return;

                content = value;

                if (content == null)
                    return;

                AutoSizeAxes = Axes.None;
                RelativeSizeAxes = Axes.None;

                RelativeSizeAxes = content.RelativeSizeAxes;
                AutoSizeAxes = content?.AutoSizeAxes ?? AutoSizeAxes;
            }
        }

        private readonly DrawablePool<T> pool;
        private readonly Action<T>? setupAction;
        private readonly double timeBeforeLoad;
        private readonly double timeBeforeUnload;

        private double timeVisible;
        private double timeHidden;

        public bool DelayedLoadCompleted { get; protected set; }

        private readonly LayoutValue<DelayedLoadWrapper.IOnScreenOptimisingContainer?> optimisingContainerCache = new LayoutValue<DelayedLoadWrapper.IOnScreenOptimisingContainer?>(Invalidation.Parent);
        private readonly LayoutValue isIntersectingCache = new LayoutValue(Invalidation.All);

        private ScheduledDelegate? isIntersectingResetDelegate;
        private ScheduledDelegate? scheduledUnloadCheckRegistration;
        private ScheduledDelegate? unloadSchedule;

        private bool shouldLoadContent => timeVisible > timeBeforeLoad;
        private bool shouldUnloadContent => timeBeforeUnload == 0 || timeHidden > timeBeforeUnload;

        private bool isIntersecting { get; set; }

        private DelayedLoadWrapper.IOnScreenOptimisingContainer? findParentOptimisingContainer() => this.FindClosestParent<DelayedLoadWrapper.IOnScreenOptimisingContainer>();

        private readonly object disposalLock = new object();
        private bool isDisposed;

        private readonly LayoutValue<IFrameBasedClock> unloadClockBacking = new LayoutValue<IFrameBasedClock>(Invalidation.Parent);

        private IFrameBasedClock unloadClock => unloadClockBacking.IsValid ? unloadClockBacking.Value : unloadClockBacking.Value = this.FindClosestParent<Game>() == null ? Game.Clock : Clock;

        public PoolableLoadUnloadWrapper(DrawablePool<T> pool, Action<T>? setupAction = null, double timeBeforeLoad = 500, double timeBeforeUnload = 2000)
        {
            this.pool = pool;
            this.setupAction = setupAction;
            this.timeBeforeLoad = timeBeforeLoad;
            this.timeBeforeUnload = timeBeforeUnload;

            AddLayout(optimisingContainerCache);
            AddLayout(isIntersectingCache);
        }

        protected override void Update()
        {
            base.Update();

            // This code can be expensive, so only run if we haven't yet loaded.
            if (DelayedLoadCompleted) return;

            if (!isIntersecting)
                timeVisible = 0;
            else
                timeVisible += Time.Elapsed;

            if (shouldLoadContent)
                PerformDelayedLoad();
        }

        protected void PerformDelayedLoad()
        {
            if (DelayedLoadCompleted)
                throw new InvalidOperationException("Load has already started!");

            InternalChild = Content = pool.Get(setupAction);
            DelayedLoadCompleted = true;

            timeVisible = 0;
            // Scheduled for another frame since Update() may not have run yet and thus OptimisingContainer may not be up-to-date
            scheduledUnloadCheckRegistration = Game.Schedule(() =>
            {
                // Since this code is running on the game scheduler, it needs to be safe against a potential simultaneous async disposal.
                lock (disposalLock)
                {
                    if (isDisposed)
                        return;

                    Debug.Assert(DelayedLoadCompleted);
                    Debug.Assert(Content.LoadState == LoadState.Loaded);

                    Debug.Assert(unloadSchedule == null);
                    unloadSchedule = Game.Scheduler.AddDelayed(checkForUnload, 0, true);
                    Debug.Assert(unloadSchedule != null);
                }
            });
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            bool result = base.OnInvalidate(invalidation, source);

            // For every invalidation, we schedule a reset of IsIntersecting to the game.
            // This is done since UpdateSubTreeMasking() may not be invoked in the current frame, as a result of presence/masking changes anywhere in our super-tree.
            // It is important that this is scheduled such that it occurs on the NEXT frame, in order to give this wrapper a chance to load its contents.
            // For example, if a parent invalidated this wrapper every frame, IsIntersecting would be false by the time Update() is run and may only become true at the very end of the frame.
            // The scheduled delegate will be cancelled if this wrapper has its UpdateSubTreeMasking() invoked, as more accurate intersections can be computed there instead.
            if (isIntersectingResetDelegate == null)
            {
                isIntersectingResetDelegate = Game.Scheduler.AddDelayed(wrapper => wrapper.isIntersecting = false, this, 0);
                result = true;
            }

            return result;
        }

        public override bool UpdateSubTreeMasking()
        {
            bool result = base.UpdateSubTreeMasking();

            // We can accurately compute intersections - the scheduled reset is no longer required.
            isIntersectingResetDelegate?.Cancel();
            isIntersectingResetDelegate = null;

            if (!isIntersectingCache.IsValid)
            {
                if (!optimisingContainerCache.IsValid)
                    optimisingContainerCache.Value = findParentOptimisingContainer();

                // The first condition is an intersection against the hierarchy, including any parents that may be masking this wrapper.
                // It is the same calculation as Drawable.IsMaskedAway, however IsMaskedAway is optimised out for some CompositeDrawables (which this wrapper is).
                // The second condition is an exact intersection against the optimising container, which further optimises rotated AABBs where the wrapper content is not visible.
                isIntersecting = ComputeMaskingBounds().IntersectsWith(ScreenSpaceDrawQuad.AABBFloat)
                                 && optimisingContainerCache.Value?.ScreenSpaceDrawQuad.Intersects(ScreenSpaceDrawQuad) != false;

                isIntersectingCache.Validate();
            }

            return result;
        }

        private void checkForUnload()
        {
            // Since this code is running on the game scheduler, it needs to be safe against a potential simultaneous async disposal.
            lock (disposalLock)
            {
                if (isDisposed)
                    return;

                // This code can be expensive, so only run if we haven't yet loaded.
                if (isIntersecting)
                    timeHidden = 0;
                else
                    timeHidden += unloadClock.ElapsedFrameTime;

                // Don't unload if we don't need to.
                if (!shouldUnloadContent)
                    return;

                // We need to dispose the content, taking into account what we know at this point in time:
                // 1: The wrapper has not been disposed. Consequently, neither has the content.
                // 2: The content has finished loading.
                // 3: The content may not have been added to the hierarchy (e.g. if this wrapper is hidden). This is dependent upon the value of DelayedLoadCompleted.
                if (DelayedLoadCompleted)
                    ClearInternal(false);

                Content = null;
                timeHidden = 0;

                // This has two important roles:
                // 1. Stopping this delegate from executing multiple times.
                // 2. If DelayedLoadCompleted = false (content not yet added to hierarchy), prevents the now disposed content from being added (e.g. if this wrapper becomes visible again).
                CancelTasks();

                // And finally, allow another load to take place.
                DelayedLoadCompleted = false;
            }
        }

        protected virtual void CancelTasks()
        {
            isIntersectingCache.Invalidate();

            if (unloadSchedule != null)
            {
                unloadSchedule.Cancel();
                unloadSchedule = null;
            }

            scheduledUnloadCheckRegistration?.Cancel();
            scheduledUnloadCheckRegistration = null;
        }

        internal override void UnbindAllBindables()
        {
            base.UnbindAllBindables();
            CancelTasks();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            lock (disposalLock)
                isDisposed = true;

            CancelTasks();
        }
    }
}
