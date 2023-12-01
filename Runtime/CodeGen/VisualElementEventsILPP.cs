using System;
using System.Linq;
using BsiGame.CodeGen;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine.UIElements;

namespace BsiGame.UI.UIElements
{
	[UsedImplicitly]
	public class VisualElementEventsILPP : ILPostProcessor
	{
		// CONSTANTS

		private const string Panel_HierarchyChanged_Callback = "Panel_HierarchyChanged_Callback";
		private const string Panel_HierarchyChanged_Delegate = "Panel_HierarchyChanged_Delegate";
		private const string StorePanel                      = "StorePanel";
		private const string TriggerEvent                    = "TriggerEvent";

		private static readonly Type VisualElement_Type          = typeof(VisualElement);
		private static readonly Type BaseVisualElementPanel_Type = Type.GetType("UnityEngine.UIElements.BaseVisualElementPanel, UnityEngine.UIElementsModule");
		private static readonly Type HierarchyEvent_Type         = Type.GetType("UnityEngine.UIElements.HierarchyEvent, UnityEngine.UIElementsModule");
		private static readonly Type HierarchyChangeType_Type    = Type.GetType("UnityEngine.UIElements.HierarchyChangeType, UnityEngine.UIElementsModule");

		// PRIVATES FIELDS

		private AssemblyDefinition assemblyDefinition;
		private ModuleDefinition   mainModule;

		// PRIVATES METHODS

		private MethodDefinition Create_Panel_HierarchyChanged_Callback_Method(TypeDefinition containingType)
		{
			/***
			 * Creates "VisualElementEvents.Panel_HierarchyChanged_Callback(UnityEngine.UIElements.VisualElement ve, UnityEngine.UIElements.HierarchyChangeType changeType)"
			 *
			 * ===
			 *
			 * private void Panel_HierarchyChanged_Callback(UnityEngine.UIElements.VisualElement ve, UnityEngine.UIElements.HierarchyChangeType changeType)
			 * {
			 *		TriggerEvent(ve, (HierarchyChangeType) (int) changeType);
			 * }
			 */

			var triggerEventMethod         = containingType.Methods.FirstOrDefault(m => m.Name == TriggerEvent);
			var visualElementTypeImp       = mainModule.ImportReference(VisualElement_Type);
			var hierarchyChangeTypeTypeImp = assemblyDefinition.MainModule.ImportReference(HierarchyChangeType_Type);

			var method          = new MethodDefinition(Panel_HierarchyChanged_Callback, MethodAttributes.Public, mainModule.TypeSystem.Void);
			var veParam         = new ParameterDefinition("ve", ParameterAttributes.None, visualElementTypeImp);
			var changeTypeParam = new ParameterDefinition("changeType", ParameterAttributes.None, hierarchyChangeTypeTypeImp);

			method.Body = new MethodBody(method);
			var il = method.Body.GetILProcessor();

			method.Parameters.Add(veParam);
			method.Parameters.Add(changeTypeParam);

			il.Append(Instruction.Create(OpCodes.Nop));
			il.Append(Instruction.Create(OpCodes.Ldarg_0));
			il.Append(Instruction.Create(OpCodes.Ldarg_1));
			il.Append(Instruction.Create(OpCodes.Ldarg_2));
			il.Append(Instruction.Create(OpCodes.Call, triggerEventMethod));
			il.Append(Instruction.Create(OpCodes.Nop));
			il.Append(Instruction.Create(OpCodes.Ret));

			return method;
		}

		private FieldDefinition Create_panel_Field()
		{
			/***
			 * Creates field "VisualElementEvents.panel"
			 *
			 * ===
			 *
			 * private UnityEngine.UIElements.BaseVisualElementPanel panel;
			 */

			var baseVisualElementPanelTypeImp = mainModule.ImportReference(BaseVisualElementPanel_Type);
			var field                         = new FieldDefinition("panel", FieldAttributes.Private, baseVisualElementPanelTypeImp);

			return field;
		}

		private void Implement_Panel_HierarchyChanged_Delegate_Method(TypeDefinition containingType, MethodDefinition callbackMethod)
		{
			/***
			 * Implements "VisualElementEvents.Panel_HierarchyChanged_Delegate()"
			 *
			 * ===
			 *
			 * private void Panel_HierarchyChanged_Delegate()
			 * {
			 *		return new UnityEngine.UIElements.HierarchyEvent(Panel_HierarchyChanged_Callback);
			 * }
			 */

			var hierarchyEventTypeImp     = mainModule.ImportReference(HierarchyEvent_Type);
			var hierarchyEventTypeRslv    = hierarchyEventTypeImp.Resolve();
			var hierarchyEventTypeCtor    = hierarchyEventTypeRslv.Methods.FirstOrDefault(m => m.Name == ".ctor");
			var hierarchyEventTypeCtorImp = mainModule.ImportReference(hierarchyEventTypeCtor);

			var method = containingType.Methods.FirstOrDefault(m => m.Name == Panel_HierarchyChanged_Delegate)!;
			var il     = method.Body.GetILProcessor();

			method.Body.Instructions.Clear();

			il.Append(Instruction.Create(OpCodes.Ldarg_0));
			il.Append(Instruction.Create(OpCodes.Ldftn, callbackMethod));
			il.Append(Instruction.Create(OpCodes.Newobj, hierarchyEventTypeCtorImp));
			il.Append(Instruction.Create(OpCodes.Ret));

			il.Body.Optimize();
		}

		private void Implement_StorePanel_Method(TypeDefinition containingType, FieldDefinition panelField)
		{
			/***
			 * Implements "VisualElement.StorePanel(IPanel panel)"
			 *
			 * ===
			 *
			 * private void StorePanel(IPanel panel)
			 * {
			 *		this.panel = (UnityEngine.UIElements.BaseVisualElementPanel) panel;
			 * }
			 */

			var storePanelMethod              = containingType.Methods.FirstOrDefault(m => m.Name == StorePanel)!;
			var baseVisualElementPanelTypeImp = mainModule.ImportReference(BaseVisualElementPanel_Type);
			var il                            = storePanelMethod.Body.GetILProcessor();

			storePanelMethod.Body.Instructions.Clear();

			il.Append(Instruction.Create(OpCodes.Nop));
			il.Append(Instruction.Create(OpCodes.Ldarg_0));
			il.Append(Instruction.Create(OpCodes.Ldarg_1));
			il.Append(Instruction.Create(OpCodes.Castclass, baseVisualElementPanelTypeImp));
			il.Append(Instruction.Create(OpCodes.Stfld, panelField));
			il.Append(Instruction.Create(OpCodes.Ret));
		}

		// INTERFACES

		public override ILPostProcessor GetInstance() => this;

		public override bool WillProcess(ICompiledAssembly compiledAssembly)
		{
			return compiledAssembly.Name == "com.bsigame.VisualElementEvents";
		}

		public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
		{
			assemblyDefinition = ILPPHelpers.OpenCompiledAssembly(compiledAssembly, out _);
			mainModule         = assemblyDefinition.MainModule;

			var visualElementEventsType = assemblyDefinition.MainModule.GetTypeDefinition(nameof(VisualElementEvents));
			var panelField              = Create_panel_Field();
			var callbackMethod          = Create_Panel_HierarchyChanged_Callback_Method(visualElementEventsType);

			visualElementEventsType.Fields.Add(panelField);
			visualElementEventsType.Methods.Add(callbackMethod);

			Implement_Panel_HierarchyChanged_Delegate_Method(visualElementEventsType, callbackMethod);
			Implement_StorePanel_Method(visualElementEventsType, panelField);

			return ILPPHelpers.WriteAssembly(assemblyDefinition);
		}
	}
}