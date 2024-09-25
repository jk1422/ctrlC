using Colossal.Logging;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;

namespace ctrlC.Tools
{
	partial class CameraTool : ToolBaseSystem
	{
		public override string toolID => "ctrlC.CameraTool";
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(CameraTool)}").SetShowsErrorsInUI(false);
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
			Enabled = false;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			Camera camera = Camera.main;
			if (camera == null)
			{
				log.Error("No main camera found");
				return;
			}

			
		}

		public void ActivateTool(float3 cetroid)
		{
			Enabled = true;

			Camera camera = Camera.main;
			if (camera == null)
			{
				log.Error("No main camera found");
				return;
			}

			

		}
	}
}
