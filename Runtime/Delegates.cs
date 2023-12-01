using UnityEngine.UIElements;

namespace BsiGame.UI.UIElements
{
	public delegate void HierarchyEvent(VisualElement visualElement, VisualElement child, HierarchyChangeType changeType);
}