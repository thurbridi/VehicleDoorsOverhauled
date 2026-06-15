using System;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class SatsumaPatcher : VehiclePatcher
  {
    private Rigidbody vehicleRigidbody;
    private Transform body;
    private InteriorLight interiorLightComponent;
    private Transform interiorLightPivot;
    private FsmBool fsmLightON;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "open_door1";
    private const string audioClipClose = "close_door1";
    private const string leftDoorName = "door left(Clone)";
    private const string rightDoorName = "door right(Clone)";
    private VehicleDoor.Axis hingeAxis = VehicleDoor.Axis.Z;
    private VehicleDoor.Axis angularVelocityAxis = VehicleDoor.Axis.Y;
    private VehicleDoor.Axis doorAngleAxis = VehicleDoor.Axis.Z;
    protected override float DefaultPlayerInteractionTorque => 80f;


    public SatsumaPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      body = vehicle.Find("Body");

      PatchInteriorLight(vehicle);
      PatchExistingDoors();
    }

    protected override void OnDoorOpened(Transform door)
    {
      interiorLightComponent.OnDoorOpened();
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
      fsmLightON.Value = interiorLightComponent.IsLightOn;
    }

    protected override void OnDoorClosed(Transform door)
    {
      interiorLightComponent.OnDoorClosed();
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
      fsmLightON.Value = interiorLightComponent.IsLightOn;
    }

    private void PatchExistingDoors()
    {
      // Installed doors are children of their respective pivot; loose doors are root objects tagged "PART".
      // "SATSUMA(557kg, 248)/Body/pivot_door_left/door left(Clone)"
      // "SATSUMA(557kg, 248)/Body/pivot_door_right/door right(Clone)"

      Transform leftDoor = body.Find("pivot_door_left/" + leftDoorName);
      if (leftDoor)
      {
        PatchDoor(leftDoor, DoorSide.Left, isInstalled: true);
      }
      else
      {
        leftDoor = GameObject.Find(leftDoorName)?.transform;
        if (leftDoor) PatchDoor(leftDoor, DoorSide.Left, isInstalled: false);
      }

      Transform rightDoor = body.Find("pivot_door_right/" + rightDoorName);
      if (rightDoor)
      {
        PatchDoor(rightDoor, DoorSide.Right, isInstalled: true);
      }
      else
      {
        rightDoor = GameObject.Find(rightDoorName)?.transform;
        if (rightDoor) PatchDoor(rightDoor, DoorSide.Right, isInstalled: false);
      }
    }

    private void PatchDoor(Transform door, DoorSide doorSide, bool isInstalled)
    {
      VehicleDoor.Config config = doorSide == DoorSide.Left
        ? CreateLeftDoorConfig(door.gameObject, vehicleRigidbody, nearClosedAngle: 5f, doorCheckAngle: 78f)
        : CreateRightDoorConfig(door.gameObject, vehicleRigidbody, nearClosedAngle: 355f, doorCheckAngle: 282f);

      config.hingeAxis = hingeAxis;
      config.angularVelocityAxis = angularVelocityAxis;
      config.doorAngleAxis = doorAngleAxis;
#if DEBUG
      config.debugLog = false;
#endif

      door.gameObject.AddComponent<VehicleDoor>();

      if (isInstalled)
      {
        door.GetPlayMaker("Use").enabled = false;
        door.GetComponent<VehicleDoor>().Initialize(config);
      }

      PlayMakerFSM boltCheckFsm = door.GetPlayMaker("BoltCheck");
      InjectBoltCheckFsm(boltCheckFsm, door, config);
    }

    private void InjectBoltCheckFsm(PlayMakerFSM boltCheckFsm, Transform door, VehicleDoor.Config config)
    {
      bool didSucceed;

      didSucceed = boltCheckFsm.FsmInject(
        stateName: "Bolts ON",
        hook: () =>
        {
          door.GetPlayMaker("Use").enabled = false;

          var component = door.GetComponent<VehicleDoor>();
          component.Initialize(config);
        }
      );
      if (!didSucceed)
      {
        ModConsole.LogError($"[VehicleDoorsOverhauled][SatsumaPatcher]: Failed to inject into 'Bolts ON' state for door {door}.");
        return;
      }

      didSucceed = boltCheckFsm.FsmInject(
        stateName: "Bolts OFF",
        hook: () =>
        {
          door.GetPlayMaker("Use").enabled = true;
          door.GetComponent<VehicleDoor>().enabled = false;
          door.gameObject.layer = LayerMask.NameToLayer("Parts");
        }
      );
      if (!didSucceed)
      {
        ModConsole.LogError($"[VehicleDoorsOverhauled][SatsumaPatcher]: Failed to inject into 'Bolts OFF' state for door {door}.");
      }
    }

    private void PatchInteriorLight(Transform vehicle)
    {
      var interiorLight = vehicle.Find("Interior/InteriorLight");
      var useFSM = interiorLight.GetPlayMaker("Use");
      interiorLightPivot = interiorLight.Find("Pivot");
      fsmLightON = useFSM.GetVariable<FsmBool>("LightON");

      useFSM.enabled = false;

      interiorLight.gameObject.layer = LayerMask.NameToLayer("Dashboard");

      // Correct switch position on load
      var pivotAngle = interiorLightPivot.localEulerAngles;
      pivotAngle.y = 0f;
      interiorLightPivot.localEulerAngles = pivotAngle;

      interiorLightComponent = interiorLight.gameObject.AddComponent<InteriorLight>();
      interiorLightComponent.Initialize(
        availablePositions: new[] {
          InteriorLight.SwitchPosition.DOORS,
          InteriorLight.SwitchPosition.ON,
          InteriorLight.SwitchPosition.OFF},
        lightObject: interiorLight.Find("Light").gameObject,
        onSwitch:
          () =>
          {
            MasterAudio.PlaySound3DAndForget(
              sType: audioGroup,
              sourceTrans: interiorLight,
              variationName: "dash_button",
              volumePercentage: 0.4f);

            var pivotAngle = interiorLightPivot.localEulerAngles;
            var yAngle = interiorLightComponent.GetSwitchPosition() switch
            {
              InteriorLight.SwitchPosition.ON => 12f,
              InteriorLight.SwitchPosition.DOORS => 0f,
              InteriorLight.SwitchPosition.OFF => -12f,
              _ => 0f,
            };
            pivotAngle.y = yAngle;
            interiorLightPivot.localEulerAngles = pivotAngle;
            fsmLightON.Value = interiorLightComponent.IsLightOn;
          });
    }
  }
}
