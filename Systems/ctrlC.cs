using Colossal.Logging;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using Game.Input;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
namespace ctrlC.Systems
{
	public partial class ctrlC : ToolBaseSystem
	{

		private InputAction _selectionTool;
		private InputAction _copyAction;
		private List<ProxyAction> proxyActions = new List<ProxyAction>();

		private SelectionTool selectionTool;
		private StampPlacementTool stampPlacementTool;
		private DefaultToolSystem defaultToolSystem;
		
		
		private ModUISystem modUISystem => World.GetOrCreateSystemManaged<ModUISystem>();

		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}").SetShowsErrorsInUI(false);

		public override string toolID => "ctrlC";

		public override PrefabBase GetPrefab()
		{
			return null;
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			return false;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			//Enabled set to false by default
			Enabled = false;

			// Setup hotkey for activating selection tool
			_selectionTool = new InputAction("SelectObject_SelectionTool", InputActionType.Button);
			_selectionTool.AddBinding("<Keyboard>/c");
			// Setup hotkey for copying selected objects
			_copyAction = new InputAction("SelectObject_Copy", InputActionType.Button);
			_copyAction.AddCompositeBinding("ButtonWithOneModifier")
					   .With("Modifier", "<Keyboard>/ctrl")
					   .With("Button", "<Keyboard>/c");

			selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
			stampPlacementTool = World.GetOrCreateSystemManaged<StampPlacementTool>();
			defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			_selectionTool.Enable();
			
		}

		public void OnKeyPressed(KeyCode code)
		{
			log.Info("onKeyPressed");
			log.Info($"apply mode: {defaultToolSystem.applyMode}");
			log.Info($"checked state ref: {defaultToolSystem.CheckedStateRef.Enabled}");
			log.Info($"info view: {defaultToolSystem.infoview}");
			try
			{
				if (code == KeyCode.C && !selectionTool.Enabled)
				{

					m_ToolSystem.selected = Entity.Null;
					m_ToolSystem.activeTool = selectionTool;
					modUISystem.SelectionToolEnabled = true;
				}
			}
			catch (Exception e)
			{
				log.Info($"exeption: {e}");
			}

		}
		public void ToggleTool(bool enable)
		{
			if (enable && m_ToolSystem.activeTool != this)
			{
				m_ToolSystem.selected = Entity.Null;
				m_ToolSystem.activeTool = this;
			}
			else if (!enable && m_ToolSystem.activeTool == this)
			{
				m_ToolSystem.selected = Entity.Null;
				m_ToolSystem.activeTool = m_DefaultToolSystem;
			}
		}
	}
}
