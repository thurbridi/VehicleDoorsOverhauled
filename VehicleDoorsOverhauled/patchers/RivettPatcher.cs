using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  enum DoorSide { Left, Right }
  static class RivettPatcher
  {
    static Rigidbody vehicleRigidbody;
    static Collider[] vehicleColliders;
    static Transform spawnersVIN, assemblies;
    private const float playerInteractionTorque = 250f;
    private const float doorCheckBreakTorque = 200f;
    private const float angularVelocityToCloseDoor = 2.2f;
    const string audioGroup = "CarFoley";
    const string audioClipOpen = "corris_door_open";
    const string audioClipClose = "corris_door_close";

    public static void Patch()
    {
      Initialize();
      PatchExistingDoors();
      PatchDoorSpawners();
    }

    static void Initialize()
    {
      spawnersVIN = GameObject.Find("CARPARTS").transform.Find("PARTSYSTEM/SPAWNERS_VIN");

      Transform vehicle = GameObject.Find("CORRIS").transform;
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      vehicleColliders = vehicle.GetComponentsInChildren<Collider>();
      assemblies = vehicle.Find("Assemblies");
    }


    static void PatchLeftDoor()
    {

    }

    static void PatchRightDoor()
    {

    }

    // Rivett is a special case because doors can be installed/removed and 0 or more doors can exist at any given time
    static void PatchDoorSpawners()
    {
      PatchLeftDoorSpawner();
      PatchRightDoorSpawner();
    }

    static void PatchLeftDoorSpawner()
    {
      Transform leftDoorSpawner = spawnersVIN.Find("DoorLeft407");
      PatchSpawner(leftDoorSpawner, DoorSide.Left);
    }

    static void PatchRightDoorSpawner()
    {
      Transform rightDoorSpawner = spawnersVIN.Find("DoorRight408");
      PatchSpawner(rightDoorSpawner, DoorSide.Right);
    }

    static void PatchSpawner(Transform spawnerTransform, DoorSide doorSide)
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

    static void PatchExistingDoors()
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

        // If door is installed, immediatly initialize the monobehaviour.
        if (door.transform.parent != null)
        {
          var useDoorFsm = door.GetPlayMaker("Use");
          useDoorFsm.enabled = false;

          // Ignore collision with vehicle body
          Collider doorCollider = door.GetComponent<Collider>();
          foreach (Collider vehicleCollider in vehicleColliders)
          {
            Physics.IgnoreCollision(doorCollider, vehicleCollider, true);
          }

          door.transform.localRotation = Quaternion.identity;

          door.GetComponent<VehicleDoor>().Initialize(config);
        }

        PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
        InjectDataFsm(dataFsm, door.transform, config);
      }
    }


    static void OnDoorOpened(Transform door)
    {
      door.parent.GetPlayMaker("Data").GetVariable<FsmBool>("DoorOpen").Value = true;

      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipOpen);
    }

    static void OnDoorClosed(Transform door)
    {
      door.parent.GetPlayMaker("Data").GetVariable<FsmBool>("DoorOpen").Value = false;

      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: door, variationName: audioClipClose);

      door.localRotation = Quaternion.identity;
      var fixedJoint = door.gameObject.AddComponent<FixedJoint>();
      fixedJoint.connectedBody = vehicleRigidbody;
      fixedJoint.breakForce = 9000f;
      fixedJoint.breakTorque = 9000f;
    }

    static bool IsLeftDoor(Transform door)
    {
      PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
      string id = PlayMakerExtensions.GetVariable<FsmString>(dataFsm, "ID").Value;
      return id.StartsWith("VIN407");
    }

    static bool IsRightDoor(Transform door)
    {
      PlayMakerFSM dataFsm = door.GetPlayMaker("Data");
      string id = PlayMakerExtensions.GetVariable<FsmString>(dataFsm, "ID").Value;
      return id.StartsWith("VIN408");
    }

    static DoorSide GetDoorSide(Transform door)
    {
      if (IsLeftDoor(door)) return DoorSide.Left;
      if (IsRightDoor(door)) return DoorSide.Right;

      ModConsole.LogError($"[VehicleDoorsReworked][RivettPatcher]: Failed to determine door side for door {door}");
      return DoorSide.Left; // Default to left door config
    }

    static VehicleDoor.Config CreateDoorConfig(Transform door, DoorSide doorSide)
    {
      bool isLeftDoor = doorSide == DoorSide.Left;
      VehicleDoor.Config config = new VehicleDoor.Config()
      {
        playerOpenTorque = isLeftDoor ? playerInteractionTorque : -playerInteractionTorque,
        playerCloseTorque = isLeftDoor ? -playerInteractionTorque : playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = isLeftDoor ? new JointLimits() { min = 0.25f, max = 80f } : new JointLimits() { min = -80f, max = -0.25f },
        closedHingeLimits = isLeftDoor ? new JointLimits() { min = 0f, max = 80f } : new JointLimits() { min = -80f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door),
        onDoorClosed = () => OnDoorClosed(door),
        isDoorNearClosedPredicate = (doorAngle) => isLeftDoor ? doorAngle <= 10f : doorAngle >= 350f,
        isPastDoorcheckAnglePredicate = (doorAngle) => isLeftDoor ? doorAngle > 79f : doorAngle < 281f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => isLeftDoor ? doorAngularVelocity <= -angularVelocityToCloseDoor : doorAngularVelocity >= angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Z,
      };

      return config;
    }

    static bool InjectDataFsm(PlayMakerFSM dataFsm, Transform door, VehicleDoor.Config config)
    {
      bool didSucceed;

      didSucceed = dataFsm.FsmInject(
        stateName: "Bolted",
        hook: () =>
        {
          door.GetPlayMaker("Use").enabled = false;

          var component = door.GetComponent<VehicleDoor>();
          component.Initialize(config);

          // Ignore collision with vehicle body
          Collider doorCollider = door.GetComponent<Collider>();
          foreach (Collider vehicleCollider in vehicleColliders)
          {
            Physics.IgnoreCollision(doorCollider, vehicleCollider, true);
          }
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

          // Re-enable collision with vehicle body
          Collider doorCollider = door.GetComponent<Collider>();
          foreach (Collider vehicleCollider in vehicleColliders)
          {
            Physics.IgnoreCollision(doorCollider, vehicleCollider, false);
          }
        }
      );
      if (!didSucceed)
      {
        ModConsole.LogError($"[VehicleDoorsReworked][RivettPatcher]: Failed to inject into Unbolted state for door {door}");
        return didSucceed;
      }

      return didSucceed;
    }
  }
}
