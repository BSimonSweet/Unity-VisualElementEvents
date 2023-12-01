using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace BsiGame.UI.UIElements
{
	public class VisualElementEvents : IDisposable
	{
		// POOL

		private static readonly ObjectPool<VisualElementEvents> pool = new(Create, Get, Release);

		private static VisualElementEvents Create()                              => new();
		private static void                Get(VisualElementEvents instance)     => instance.disposed = false;
		private static void                Release(VisualElementEvents instance) => instance.Dispose();

		public static PooledObject<VisualElementEvents> GetPooled(VisualElement visualElement, out VisualElementEvents instance)
		{
			var pooledObject = pool.Get(out instance);

			instance.visualElement = visualElement;
			instance.Init();

			return pooledObject;
		}

		// CONSTANTS

		private static readonly Type      BaseVisualElementPanel_Type = Type.GetType("UnityEngine.UIElements.BaseVisualElementPanel, UnityEngine.UIElementsModule");
		private static readonly EventInfo hierarchyChanged_Event      = BaseVisualElementPanel_Type.GetEvent("hierarchyChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

		// PRIVATES FIELDS

		private readonly EventCallback<AttachToPanelEvent>   AttachToPanel_d;
		private readonly EventCallback<DetachFromPanelEvent> DetachFromPanel_d;
		private readonly object[]                            hierarchyChanged_Args;

		private VisualElement visualElement;
		private object        nonTypedPanel;
		private bool          disposed;

		/***
		 * ILPP fields
		 *
		 * private UnityEngine.UIElements.BaseVisualElementPanel panel;
		 */

		// EVENTS

		public event HierarchyEvent Callbacks;

		// CONSTRUCTORS

		private VisualElementEvents()
		{
			AttachToPanel_d       = AttachToPanel;
			DetachFromPanel_d     = DetachFromPanel;
			hierarchyChanged_Args = new object[] { Panel_HierarchyChanged_Delegate() };
		}

		public VisualElementEvents(VisualElement visualElement) : this()
		{
			this.visualElement = visualElement;

			Init();
		}

		// EVENTS CALLBACKS

		private void AttachToPanel(AttachToPanelEvent evt)     => Init();
		private void DetachFromPanel(DetachFromPanelEvent evt) => Clear();

		// INTERFACES

		public void Dispose()
		{
			if(disposed)
				return;

			Clear();
			visualElement.UnregisterCallback(AttachToPanel_d);
			visualElement.UnregisterCallback(DetachFromPanel_d);

			Callbacks = null;
			disposed  = true;
		}

		// PRIVATES METHODS

		private void Init()
		{
			visualElement.RegisterCallback(AttachToPanel_d);
			visualElement.RegisterCallback(DetachFromPanel_d);

			if(visualElement.panel != null)
			{
				nonTypedPanel = visualElement.panel;

				StorePanel(visualElement.panel);
				RegisterCallback();
			}
		}

		private void Clear()
		{
			if(nonTypedPanel == null)
				return;

			UnregisterCallback();

			nonTypedPanel = null;
			// TODO clear typed panel
		}

		private void RegisterCallback()
		{
			hierarchyChanged_Event.AddMethod.Invoke(nonTypedPanel, hierarchyChanged_Args);
		}

		private void UnregisterCallback()
		{
			hierarchyChanged_Event.RemoveMethod.Invoke(nonTypedPanel, hierarchyChanged_Args);
		}

		[UsedImplicitly]
		private void TriggerEvent(VisualElement child, HierarchyChangeType changeType)
		{
			if(child == visualElement || visualElement.Contains(child) == false)
				return;

			Callbacks?.Invoke(visualElement, child, changeType);
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void StorePanel(IPanel panel)
		{
			/***
			 * ILPP method
			 *
			 * this.panel = (UnityEngine.UIElements.BaseVisualElementPanel) panel;
			 */
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private Delegate Panel_HierarchyChanged_Delegate()
		{
			/***
			 * ILPP method
			 *
			 * return new UnityEngine.UIElements.HierarchyEvent(Panel_HierarchyChanged_Callback);
			 */

			return null;
		}

		/***
		 * ILPP method
		 *
		 * private void Panel_HierarchyChanged_Callback(UnityEngine.UIElements.VisualElement ve, UnityEngine.UIElements.HierarchyChangeType changeType)
		 * {
		 *		TriggerEvent(ve, (HierarchyChangeType) (int) changeType);
		 * }
		 */
	}
}