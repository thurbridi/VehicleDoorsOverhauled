using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public class RivettPatcher : VehiclePatcher
  {
    private Rigidbody vehicleRigidbody;
    private Transform spawnersVIN, assemblies;
    private InteriorLight interiorLightComponent;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "corris_door_open";
    private const string audioClipClose = "corris_door_close";
    protected override float DefaultPlayerInteractionTorque => 100f;
    protected override float DefaultDoorCheckBreakTorque => 105f;
    protected override float DefaultAngularVelocityToCloseDoor => 2f;

    public RivettPatcher(string vehicleName, Func<Transform> vehicleResolver) : base(vehicleName, vehicleResolver) { }

    public override void Patch()
    {
      spawnersVIN = GameObject.Find("CARPARTS").transform.Find("PARTSYSTEM/SPAWNERS_VIN");

      Transform vehicle = FindVehicle();
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      // vehicleColliders = vehicle.GetComponentsInChildren<Collider>();
      assemblies = vehicle.Find("Assemblies");

      PatchExistingDoors();
      PatchDoorSpawners();
      PatchInteriorLight(vehicle);
    }

    protected override void OnDoorOpened(Transform door)
    {
      door.parent.GetPlayMaker("Data").GetVariable<FsmBool>("DoorOpen").Value = true; // Don't know what this is used for, just copied from vanilla fsm
      interiorLightComponent.OnDoorOpened();

      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
    }

    protected override void OnDoorClosed(Transform door)
    {
      door.parent.GetPlayMaker("Data").GetVariable<FsmBool>("DoorOpen").Value = false;
      interiorLightComponent.OnDoorClosed();

      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);

      door.localRotation = Quaternion.identity;
      var fixedJoint = door.gameObject.AddComponent<FixedJoint>();
      fixedJoint.connectedBody = vehicleRigidbody;
      fixedJoint.breakForce = 9000f;
      fixedJoint.breakTorque = 9000f;
    }

    // Rivett is a special case because doors can be installed/removed and 0 or more doors can exist at any given time
    private void PatchDoorSpawners()
    {
      PatchLeftDoorSpawner();
      PatchRightDoorSpawner();
    }

    private void PatchLeftDoorSpawner()
    {
      Transform leftDoorSpawner = spawnersVIN.Find("DoorLeft407");
      PatchSpawner(leftDoorSpawner, DoorSide.Left);
    }

    private void PatchRightDoorSpawner()
    {
      Transform rightDoorSpawner = spawnersVIN.Find("DoorRight408");
      PatchSpawner(rightDoorSpawner, DoorSide.Right);
    }

    private void PatchSpawner(Transform spawnerTransform, DoorSide doorSide)
    {
      PlayMakerFSM spawner = spawnerTransform.GetPlayMaker("Spawn");
      spawner.FsmInject(stateName: "Create product", hook: () =>
      {
        GameObject newDoor = spawner.GetVariable<FsmGameObject>("New").Value;

        var config = CreateDoorConfig(newDoor.transform, doorSide);
        if (config == null) return;

        var doorComponent = newDoor.AddComponent<VehicleDoor>();
        PlayMakerFSM dataFsm = newDoor.GetPlayMaker("Data");
        InjectDataFsm(dataFsm, newDoor.transform, config);
      }, everyFrame: false);
    }

    private void PatchExistingDoors()
    {
      // There are two places in the scene hierarchy where doors can be found:
      // loose doors are root objects, and installed doors are children of rivett
      // "CORRIS/Assemblies/VINP_DoorLeft/Door(VINXX)"
      // "CORRIS/Assemblies/VINP_DoorRight/Door(VINXX)"

      List<GameObject> looseDoors = new List<GameObject>(GameObject.FindGameObjectsWithTag("PART")).FindAll(part => part.name == "Door(VINXX)");

      // Note: Tag is lost when part is installed, so installed doors are not included
      Transform leftDoorTransform = assemblies.Find("VINP_DoorLeft/Door(VINXX)");
      Transform rightDoorTransform = assemblies.Find("VINP_DoorRight/Door(VINXX)");

      var allDoors = new List<GameObject>(looseDoors);
      if (leftDoorTransform != null) allDoors.Add(leftDoorTransform.gameObject);
      if (rightDoorTransform != null) allDoors.Add(rightDoorTransform.gameObject);

      foreach (GameObject door in allDoors)
      {
        VehicleDoor.Config config = CreateDoorConfig(door.transform, GetDoorSide(door.transform));
        if (config == null) continue;

        var doorComponent = door.AddComponent<VehicleDoor>();

        // If door is installed, immediately initialize the monobehaviour.
        if (door.transform.parent != null)
        {
          var useDoorFsm = door.GetPlayMaker("Use");
          useDoorFsm.enabled = false;

          door.GetComponent<VehicleDoor>().Initialize(config);
        }

        PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
        InjectDataFsm(dataFsm, door.transform, config);
      }
    }

    private bool IsLeftDoor(Transform door)
    {
      PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
      string id = dataFsm.GetVariable<FsmString>("ID").Value;
      return id.StartsWith("VIN407");
    }

    private bool IsRightDoor(Transform door)
    {
      PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
      string id = dataFsm.GetVariable<FsmString>("ID").Value;
      return id.StartsWith("VIN408");
    }

    private DoorSide GetDoorSide(Transform door)
    {
      if (IsLeftDoor(door)) return DoorSide.Left;
      if (IsRightDoor(door)) return DoorSide.Right;

      ModConsole.LogError($"[VehicleDoorsReworked][RivettPatcher]: Failed to determine door side for door {door}");
      return DoorSide.Left; // Default to left door config
    }

    private VehicleDoor.Config CreateDoorConfig(Transform door, DoorSide doorSide)
    {
      bool isLeftDoor = doorSide == DoorSide.Left;
      VehicleDoor.Config config = new VehicleDoor.Config()
      {
        playerOpenTorque = isLeftDoor ? PlayerInteractionTorque : -PlayerInteractionTorque,
        playerCloseTorque = isLeftDoor ? -PlayerInteractionTorque : PlayerInteractionTorque,
        doorCheckBreakTorque = DoorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = isLeftDoor ? new JointLimits() { min = 0.25f, max = 80f } : new JointLimits() { min = -80f, max = -0.25f },
        closedHingeLimits = isLeftDoor ? new JointLimits() { min = 0f, max = 80f } : new JointLimits() { min = -80f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door),
        onDoorClosed = () => OnDoorClosed(door),
        isDoorNearClosedPredicate = (doorAngle) => isLeftDoor ? doorAngle <= 10f : doorAngle >= 350f,
        isPastDoorcheckAnglePredicate = (doorAngle) => isLeftDoor ? doorAngle > 79f : doorAngle < 281f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => isLeftDoor ? doorAngularVelocity <= -AngularVelocityToCloseDoor : doorAngularVelocity >= AngularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Z,
      };
      RegisterConfigUpdater(config, doorSide);
      return config;
    }

    private bool InjectDataFsm(PlayMakerFSM dataFsm, Transform door, VehicleDoor.Config config)
    {
      bool didSucceed;

      didSucceed = dataFsm.FsmInject(
        stateName: "Bolted",
        hook: () =>
        {
          door.GetPlayMaker("Use").enabled = false;

          var component = door.GetComponent<VehicleDoor>();
          component.Initialize(config);
        }
      );
      if (!didSucceed)
      {
        ModConsole.LogError($"[VehicleDoorsReworked][RivettPatcher]: Failed to inject into Bolted state for door {door}");
        return didSucceed;
      }

      didSucceed = dataFsm.FsmInject(
        stateName: "Unbolted",
        hook: () =>
        {
          door.GetPlayMaker("Use").enabled = true;
          door.GetComponent<VehicleDoor>().enabled = false;
          door.gameObject.layer = LayerMask.NameToLayer("Parts");
          interiorLightComponent.OnDoorClosed();
        }
      );
      if (!didSucceed)

      {
        ModConsole.LogError($"[VehicleDoorsReworked][RivettPatcher]: Failed to inject into Unbolted state for door {door}");
        return didSucceed;
      }

      return didSucceed;
    }

    private void PatchInteriorLight(Transform vehicle)
    {
      var interiorLight = vehicle.Find("InteriorLight");
      var interiorLightUse = interiorLight.Find("Use");

      interiorLightUse.GetPlayMaker("Use").enabled = false;

      interiorLightUse.gameObject.layer = LayerMask.NameToLayer("Dashboard");

      interiorLightComponent = interiorLightUse.gameObject.AddComponent<InteriorLight>();
      interiorLightComponent.Initialize(
        availablePositions: new[] {
          InteriorLight.SwitchPosition.DOORS,
          InteriorLight.SwitchPosition.ON,
          InteriorLight.SwitchPosition.OFF},
        lightObject: interiorLight.Find("Electrics").gameObject,
        onSwitch:
          () => MasterAudio.PlaySound3DAndForget(
            sType: audioGroup,
            sourceTrans: interiorLightUse,
            variationName: "dash_button",
            volumePercentage: 0.4f));
    }
  }
}
