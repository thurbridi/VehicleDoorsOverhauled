using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class BachglotzPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private PlayMakerFSM interiorLightFsm;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "bach_door_open";
    private const string audioClipClose = "bach_door_close";

    public BachglotzPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("DriverDoors");
      interiorLightFsm = vehicle.Find("LOD/InteriorLight/Use").GetComponent<PlayMakerFSM>();

      PatchLeftDoor();
      PatchRightDoor();
    }

    protected override void OnDoorOpened(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
      interiorLightFsm.SendEvent("DOOROPEN");
    }

    protected override void OnDoorClosed(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
      interiorLightFsm.SendEvent("DOORCLOSE");
    }

    private void PatchLeftDoor()
    {
      Transform door = doors.Find("door(leftx)");
      Transform doorHandle = door.Find("doors/Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchRightDoor()
    {
      Transform door = doors.Find("door(right)");
      Transform doorHandle = door.Find("doors/Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }
  }
}
