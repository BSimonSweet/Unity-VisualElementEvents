using BsiGame.UI.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BsiGame.Example
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(UIDocument))]
	[AddComponentMenu("")]
	public class VisualElementEventsExample : MonoBehaviour
	{
		// PRIVATES FIELDS

		private UIDocument uiDocument;

		// LIFE CYCLE

		private void Awake()
		{
			uiDocument = GetComponent<UIDocument>();
		}

		private void Start()
		{
			var root = uiDocument.rootVisualElement;

			// You must add this Manipulator to listen to ChildChangeEvent.
			root.AddManipulator(new ChildChangeManipulator());
			root.RegisterCallback<ChildChangeEvent>(OnChildChanged);

			var childA = new VisualElement { name = "ChildA" };
			var childB = new VisualElement { name = "ChildB" };
			var childC = new VisualElement { name = "ChildC" };

			/***
			 * The parent that receives the new element must be inside a UIDocument.
			 * For example, ChildChangeEvent won't be fired when adding "childB" into "childA",
			 * because "childA" is not yet in the UIDocument.
			 */
			childA.Add(childB);

			// Will trigger the event.
			root.Add(childA);
			childA.Add(childC);

			root.Remove(childA);
			childA.Remove(childC);

			// Won't trigger the event, because "childA" is no longer in the UIDocument.
			childA.Remove(childB);
		}

		// EVENTS CALLBACKS

		private void OnChildChanged(ChildChangeEvent evt)
		{
			/***
			 * The event bubble-up to the element that have the ChildChangeManipulator.
			 * So you will receive a callback for all children and sub-children of the element.
			 */

			var isAdd  = evt.newChildCount > evt.previousChildCount;
			var parent = evt.targetParent;
			var child  = evt.targetChild;

			if(isAdd)
				Debug.Log($"Add '{child.name}' into '{parent.name}'");
			else
				Debug.Log($"Remove '{child.name}' from '{parent.name}'");
		}
	}
}