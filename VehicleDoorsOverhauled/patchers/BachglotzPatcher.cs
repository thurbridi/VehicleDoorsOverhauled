using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class BachglotzPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private InteriorLight interiorLightComponent;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "bach_door_open";
    private const string audioClipClose = "bach_door_close";
    protected override float DefaultAngularVelocityToCloseDoor => 1.8f;

    public BachglotzPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("DriverDoors");

      PatchLeftDoor();
      PatchRightDoor();
      PatchInteriorLight(vehicle);
    }

    protected override void OnDoorOpened(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
      interiorLightComponent.OnDoorOpened();
    }

    protected override void OnDoorClosed(Transform door)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
      interiorLightComponent.OnDoorClosed();
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

    private void PatchInteriorLight(Transform vehicle)
    {
      var interiorLight = vehicle.Find("LOD/InteriorLight");
      var interiorLightUse = interiorLight.Find("Use");

      interiorLightUse.GetPlayMaker("Use").enabled = false;
      interiorLightUse.gameObject.layer = LayerMask.NameToLayer("Dashboard");

      interiorLightComponent = interiorLightUse.gameObject.AddComponent<InteriorLight>();
      interiorLightComponent.Initialize(
        availablePositions: new[] {
          InteriorLight.SwitchPosition.DOORS,
          InteriorLight.SwitchPosition.ON,
          InteriorLight.SwitchPosition.OFF},
        lightObject: interiorLight.Find("Light").gameObject,
        onSwitch: () => MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: interiorLightUse, variationName: "dash_button", volumePercentage: 0.4f));
    }
  }
}
