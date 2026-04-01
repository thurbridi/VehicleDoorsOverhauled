using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class FittanPatcher : VehiclePatcher
  {
    private Transform door;
    private Transform doorHandle;
    private Rigidbody vehicleRigidbody;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "open_door1";
    private const string audioClipClose = "close_door1";

    public FittanPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      door = vehicle.Find("DriverDoors/doorr");
      doorHandle = vehicle.Find("DriverDoors/doorr/Pivot/Handle");

      PatchRightDoor();
    }

    protected override void OnDoorOpened(Transform d)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: d, variationName: audioClipOpen);
    }

    protected override void OnDoorClosed(Transform d)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: d, variationName: audioClipClose);
    }

    private void PatchRightDoor()
    {
      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }
  }
}
