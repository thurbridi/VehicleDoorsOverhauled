using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class MSCGifuPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "open_door1";
    private const string audioClipClose = "close_door1";
    protected override float DefaultPlayerInteractionTorque => 25f;
    protected override float DefaultDoorCheckBreakTorque => 85f;
    protected override float DefaultAngularVelocityToCloseDoor => 2.8f;
    protected override float DefaultStaticFrictionTorque => 1.6f;
    protected override float DefaultDynamicFrictionTorque => 0.8f;

    public MSCGifuPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("DriverDoors");

      PatchLeftDoor();
      PatchRightDoor();
    }

    protected override void OnDoorOpened(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
    }

    protected override void OnDoorClosed(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
    }

    private void PatchLeftDoor()
    {
      Transform door = doors.Find("doorl");
      PlayMakerFSM useDoorFsm = door.GetPlayMaker("Use");

      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = door.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody, doorCheckAngle: 349f));
    }

    private void PatchRightDoor()
    {
      Transform door = doors.Find("doorr");
      PlayMakerFSM useDoorFsm = door.GetPlayMaker("Use");

      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = door.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody, doorCheckAngle: 191f));
    }
  }
}
