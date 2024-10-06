using ctrlC.Components;
using Game.Net;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace ctrlC.Systems
{
    internal partial class CopySystem
	{
		private static void ComputeBaseHeightAndCentroid(List<Entity> entities, EntityManager entityManager)
		{
			foreach (var selected in entities)
			{
				if (entityManager.HasComponent<Edge>(selected) || entityManager.HasComponent<Curve>(selected))
				{
					var curve = entityManager.GetComponentData<Curve>(selected);
					if (entityManager.HasComponent<Game.Net.Elevation>(selected))
					{
						var elevation = entityManager.GetComponentData<Game.Net.Elevation>(selected).m_Elevation;
						if (elevation.x < 0 || elevation.y < 0)
							continue;
					}

					baseHeight = math.min(baseHeight, curve.m_Bezier.a.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.b.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.c.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.d.y);

					centroid += curve.m_Bezier.a + curve.m_Bezier.b + curve.m_Bezier.c + curve.m_Bezier.d;
					pointCount += 4;
				}
				else if (entityManager.HasComponent<Game.Objects.Transform>(selected))
				{
					centroid += entityManager.GetComponentData<Game.Objects.Transform>(selected).m_Position;
					pointCount++;
				}
				else if (entityManager.HasComponent<Game.Areas.Node>(selected))
				{
					var nodesBuffer = entityManager.GetBuffer<Game.Areas.Node>(selected);
					foreach (var node in nodesBuffer)
					{
						centroid += node.m_Position;
						pointCount++;
					}
				}
			}

			if (pointCount > 0)
				centroid /= pointCount;
		}

		private static void AddPrefabToSystem(PrefabSystem prefabSystem, AssetStampPrefab assetStamp)
		{
			try
			{
				prefabSystem.AddPrefab(assetStamp);
				prefabSystem.Update();
			}
			catch (Exception ex)
			{
				log.Error($"Error when trying to add Prefab: {ex.Message}");
				throw;
			}
		}
	}
}
