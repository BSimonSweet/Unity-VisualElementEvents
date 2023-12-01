using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace BsiGame.UI.UIElements
{
	public class ChildChangeEvent : EventBase<ChildChangeEvent>, IChangeEvent
	{
		// ACCESSORS

		public VisualElement targetParent       { get; protected set; }
		public VisualElement targetChild        { get; protected set; }
		public int           previousChildCount { get; protected set; }
		public int           newChildCount      { get; protected set; }

		// CONSTRUCTORS

		public ChildChangeEvent() => LocalInit();

		// PRIVATES METHODS

		protected override void Init()
		{
			base.Init();
			LocalInit();
		}

		private void LocalInit()
		{
			previousChildCount = 0;
			newChildCount      = 0;
			bubbles            = false;
			tricklesDown       = false;
		}

		// STATIC INTERFACES

		public static ChildChangeEvent GetPooled(VisualElement targetChild, int previousValue, int newValue)
		{
			var pooled = GetPooled();
			pooled.targetParent       = targetChild.parent;
			pooled.targetChild        = targetChild;
			pooled.previousChildCount = previousValue;
			pooled.newChildCount      = newValue;
			return pooled;
		}
	}

	public class ChildChangeManipulator : IManipulator
	{
		// POOL

		private static readonly ObjectPool<ChildChangeManipulator> pool = new(Create, actionOnRelease: Release);

		private static ChildChangeManipulator Create()                                 => new();
		private static void                   Release(ChildChangeManipulator instance) => instance.Clear();

		public static PooledObject<ChildChangeManipulator> GetPooled(out ChildChangeManipulator instance)
			=> pool.Get(out instance);

		// PRIVATES FIELDS

		private PooledObject<VisualElementEvents>? pooledEvents;
		private HierarchyEvent                     EventsOnCallbacks_d;
		private VisualElement                      _target;

		// ACCESSORS

		public VisualElement target
		{
			get => _target;
			set
			{
				pooledEvents.Release();

				_target = value;

				if(_target != null)
				{
					pooledEvents     =  VisualElementEvents.GetPooled(target, out var events);
					events.Callbacks += EventsOnCallbacks_d ??= EventsOnCallbacks;
				}
			}
		}

		// PRIVATES METHODS

		private void Clear()
		{
			pooledEvents.Release();
			_target = null;
		}

		// EVENTS CALLBACKS

		private void EventsOnCallbacks(VisualElement visualElement, VisualElement child, HierarchyChangeType changeType)
		{
			/**
			 * NOTE visualElement.childCount can be 0 if its .Clear() method has been called
			 */

			var parent = child.parent;
			var currentChildCount = changeType switch
			{
				HierarchyChangeType.Remove => parent.childCount - 1,
				_                          => parent.childCount
			};
			var previousChildCount = changeType switch
			{
				HierarchyChangeType.Add => parent.childCount - 1,
				_                       => parent.childCount
			};

			using var e = ChildChangeEvent.GetPooled(child, previousChildCount, Mathf.Clamp(currentChildCount, 0, currentChildCount));
			e.target = visualElement;

			_target.SendEvent(e);
		}
	}
}