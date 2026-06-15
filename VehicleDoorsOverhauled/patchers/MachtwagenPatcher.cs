using System;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class MachtwagenPatcher : VehiclePatcher
  {
    private Transform doors;
    private Rigidbody vehicleRigidbody;
    private InteriorLight interiorLightComponent;
    private Transform domeSwitchMesh;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "taxi_door_open";
    private const string audioClipClose = "taxi_door_close";

    public MachtwagenPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

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

      // Keep NPC interaction — only disable player-facing actions
      useDoorFsm.GetState("Mouse off").Actions[0].Enabled = false;
      useDoorFsm.GetState("Mouse over 1").Actions[0].Enabled = false;
      useDoorFsm.GetState("Mouse over 1").Actions[2].Enabled = false;

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

      // Keep NPC interaction — only disable player-facing actions
      useDoorFsm.GetState("Mouse off").Actions[0].Enabled = false;
      useDoorFsm.GetState("Mouse over 1").Actions[0].Enabled = false;
      useDoorFsm.GetState("Mouse over 1").Actions[2].Enabled = false;

      VehicleDoor doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(CreateRightDoorConfig(door.gameObject, vehicleRigidbody));
    }

    private void PatchInteriorLight(Transform vehicle)
    {
      var buttonDome = vehicle.Find("Functions/Dashboard/Buttons/ButtonDome");
      var interiorLight = vehicle.Find("LOD/InteriorLight");
      domeSwitchMesh = vehicle.Find("Functions/Dashboard/Buttons/PivotDome/mesh");

      buttonDome.GetPlayMaker("Use").enabled = false;
      buttonDome.gameObject.layer = LayerMask.NameToLayer("Dashboard");

      interiorLightComponent = buttonDome.gameObject.AddComponent<InteriorLight>();
      interiorLightComponent.Initialize(
        availablePositions: new[] {
          InteriorLight.SwitchPosition.DOORS,
          InteriorLight.SwitchPosition.ON},
        lightObject: interiorLight.Find("Light").gameObject,
        interactionLabel: "Dome light",
        onSwitch: () =>
        {
          MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: buttonDome, variationName: "taxi_dash_switch", volumePercentage: 1f);
          var angles = domeSwitchMesh.localEulerAngles;
          angles.x = interiorLightComponent.GetSwitchPosition() == InteriorLight.SwitchPosition.DOORS ? 0f : -25f;
          domeSwitchMesh.localEulerAngles = angles;
        });
    }
  }
}