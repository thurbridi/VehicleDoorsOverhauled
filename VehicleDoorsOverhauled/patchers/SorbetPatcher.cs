using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class SorbetPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private InteriorLight interiorLightComponent;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "sorbet_door_open";
    private const string audioClipClose = "sorbet_door_close";

    public SorbetPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("Doors");

      PatchFLDoor();
      PatchFRDoor();
      PatchRLDoor();
      PatchRRDoor();
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

    private void PatchFLDoor()
    {
      Transform door = doors.Find("DoorFront(leftx)");
      Transform doorHandle = door.Find("FrontL/PlayerColl/Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchFRDoor()
    {
      Transform door = doors.Find("DoorFront(right)");
      Transform doorHandle = door.Find("FrontR/PlayerColl/Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchRLDoor()
    {
      Transform door = doors.Find("DoorRear(leftx)");
      Transform doorHandle = door.Find("RearL/PlayerColl/Handle");

      PlayMakerFSM useDoorFsm = doorHandle.GetPlayMaker("Use");
      useDoorFsm.enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateLeftDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchRRDoor()
    {
      Transform door = doors.Find("DoorRear(right)");
      Transform doorHandle = door.Find("RearR/PlayerColl/Handle");

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