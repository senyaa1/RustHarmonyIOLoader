using System;
using System.Collections.Generic;
using UnityEngine;

namespace HarmonyIOLoader
{
	class IOEntityToWheelSwitchConnection : MonoBehaviour
	{
		private IOEntity ioEntity;
		private List<WheelSwitch> wheelSwitches = new List<WheelSwitch>();
		private bool isEnabled;
		private BaseEntity.Flags flags;

		private void Awake()
		{
			enabled = false;
			ioEntity = GetComponent<IOEntity>();
			flags = ((BaseEntity.Flags)((ioEntity is WheelSwitch) ? 128 : 2));
		}

		private void Update()
		{
			if (isEnabled)
			{
				if (!ioEntity.HasFlag(flags))
				{
					isEnabled = false;
					for (int i = 0; i < wheelSwitches.Count; i++)
					{
						WheelSwitch wheelSwitch = wheelSwitches[i];
						if (wheelSwitch != null && wheelSwitch.IsInvoking(new Action(wheelSwitch.Powered)))
						{
							wheelSwitch.CancelInvoke(new Action(wheelSwitch.Powered));
						}
					}
				}
			}
			else if (ioEntity.HasFlag(flags))
			{
				isEnabled = true;
				for (int j = 0; j < wheelSwitches.Count; j++)
				{
					WheelSwitch wheelSwitch2 = wheelSwitches[j];
					if (wheelSwitch2 != null)
					{
						wheelSwitch2.InvokeRepeating(new Action(wheelSwitch2.Powered), 0f, 0.1f);
					}
				}
			}
		}

		public void SetTarget(WheelSwitch target)
		{
			if (!(target == null))
			{
				wheelSwitches.Add(target);
				enabled = true;
			}
		}
	}
}
