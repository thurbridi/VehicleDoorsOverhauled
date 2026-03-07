using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class KekmetPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "car_old_door_open";
    private const string audioClipClose = "car_old_door_close";

    public KekmetPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

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
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchRightDoor()
    {
      Transform door = doors.Find("doorr");

      PlayMakerFSM useDoorFsm = door.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = door.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }
  }
}
