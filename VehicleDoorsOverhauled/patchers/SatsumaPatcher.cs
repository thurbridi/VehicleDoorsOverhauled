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
    private PlayMakerFSM interiorLightFsm;
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
      interiorLightFsm = vehicle.Find("Interior/InteriorLight").GetPlayMaker("Use");

      PatchExistingDoors();
    }

    protected override void OnDoorOpened(Transform door)
    {
      interiorLightFsm.GetVariable<FsmBool>("DoorOpen").Value = true;
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
    }

    protected override void OnDoorClosed(Transform door)
    {
      interiorLightFsm.GetVariable<FsmBool>("DoorOpen").Value = false;
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);
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
  }
}
