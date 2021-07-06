using System;


namespace HarmonyIOLoader
{
    class AutoTurretManager : FacepunchBehaviour
    {
		public bool UnlimitedAmmo { get; private set; }
		private AutoTurret AutoTurret;

		private void Awake()
		{
			AutoTurret = GetComponent<AutoTurret>();
			enabled = false;
		}

		public void Initialize(bool unlimitedAmmo, bool peacekeeperMode, string weaponName)
		{
			if (AutoTurret == null) return;
			UnlimitedAmmo = unlimitedAmmo;
			AutoTurret.SetPeacekeepermode(peacekeeperMode);
			if (!string.IsNullOrEmpty(weaponName))
			{
				Item item = ItemManager.CreateByName(weaponName, 1, 0UL);
				item.RemoveFromContainer();
				item.position = 0;
				item.SetParent(AutoTurret.inventory);
				AutoTurret.inventory.MarkDirty();
				item.MarkDirty();

				if (!AutoTurret.IsInvoking(new Action(AutoTurret.UpdateAttachedWeapon)))
					AutoTurret.Invoke(new Action(AutoTurret.UpdateAttachedWeapon), 0.5f);
				if (unlimitedAmmo)
				{
					var weapon = AutoTurret.GetAttachedWeapon();
					if(weapon != null)
						weapon.primaryMagazine.contents = weapon.primaryMagazine.capacity;
					AutoTurret.Invoke(new Action(RefillAmmo), 1f);
				}
			}
		}

		private void RefillAmmo()
		{
			if (AutoTurret == null) return;
			var weapon = AutoTurret.GetAttachedWeapon();

			if (!(weapon == null))
			{
				int? ammoID;
				if (weapon == null)
				{
					ammoID = null;
				}
				else
				{
					ItemDefinition ammoType = weapon.primaryMagazine.ammoType;
					ammoID = ((ammoType != null) ? new int?(ammoType.itemid) : null);
				}
				if (AutoTurret.inventory.GetAmount(ammoID.GetValueOrDefault(), true) < 1)
				{
					Item item = ItemManager.CreateByItemID(ammoID.GetValueOrDefault(), 128, 0UL);
					item.MoveToContainer(AutoTurret.inventory, -1, true, true);
				}
			}
		}
	}
}
