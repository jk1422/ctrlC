using Game.Common;
using Game.Tools;
using Unity.Entities;

namespace ctrlC.Utils
{
    public static class Highlighter
	{
		internal static void ChangeHighlighting_MainThread(this EntityManager entityManager, Entity entity, ChangeMode mode)
		{
			if (entity == Entity.Null || !entityManager.Exists(entity))
			{
				return;
			}
			bool changed = false;
			if (mode == ChangeMode.AddHighlight && !entityManager.HasComponent<Highlighted>(entity))
			{
				entityManager.AddComponent<Highlighted>(entity);
				changed = true;
			}
			else if (mode == ChangeMode.RemoveHighlight && entityManager.HasComponent<Highlighted>(entity))
			{
				entityManager.RemoveComponent<Highlighted>(entity);
				changed = true;
			}
			if (changed && !entityManager.HasComponent<BatchesUpdated>(entity))
			{
				entityManager.AddComponent<BatchesUpdated>(entity);
			}
		}

		internal enum ChangeMode
		{
			AddHighlight,
			RemoveHighlight,
		}
	}
}
