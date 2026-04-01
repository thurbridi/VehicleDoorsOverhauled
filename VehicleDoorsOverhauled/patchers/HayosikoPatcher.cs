using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class HayosikoPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private PlayMakerFSM interiorLightFsm;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "open_door1";
    private const string audioClipClose = "close_door1";
    protected override float DefaultAngularVelocityToCloseDoor => 1.8f;

    public HayosikoPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

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
      Transform door = doors.Find("doorl");
      Transform doorHandle = door.Find("Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchRightDoor()
    {
      Transform door = doors.Find("doorr");
      Transform doorHandle = door.Find("Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }
  }
}
