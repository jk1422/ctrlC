using Colossal.Logging;
using ctrlC.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

//namespace ctrlC.Utils
//{
//	public class KeyListener : MonoBehaviour
//	{
//		private readonly float _clickInterval = 0.3f;
//		private bool _hit;
//		private float _lastClicked;
//		private KeyCode _code;
//		public HashSet<KeyCode> _codes;
//		public event Action<KeyCode> KeyHitEvent = delegate { };
//		
//
//		public void Awake()
//		{
//			_codes = new HashSet<KeyCode> { KeyCode.C };
//
//		}
//
//		public void OnGUI()
//		{
//			if (Event.current.type == EventType.KeyDown)
//			{
//				if (IsCtrlC(Event.current) && Time.time - _lastClicked > _clickInterval)
//				{
//					
//					_code = Event.current.keyCode;
//					_lastClicked = Time.time;
//					_hit = true;
//				}
//			}
//		}
//
//		public void Update()
//		{
//			if (_hit)
//			{
//				_hit = false;
//				KeyHitEvent(_code);
//			}
//		}
//
//		public void OnDestroy()
//		{
//			KeyHitEvent = null;
//		}
//
//		private bool IsCtrlC(Event currentEvent)
//		{
//			return currentEvent.keyCode == KeyCode.C && (currentEvent.control || currentEvent.command);
//		}
//	}
//}
