# Co-Experiencing Virtual Spaces: Standalone Multiuser Virtual Reality Tracking with Oculus Quest in Unity

![alt text](README_Pictures/EchoLocation_Screenshot2.png)

## Introduction

Co-Experiencing Virtual Spaces is a framework in the game engine Unity which allows co-location tracking of multiple VR glasses in the same real space as well as in the same virtual space. By an initial calibration of the devices with the help of the controllers in the center of the room, an absolute zero point in the room can be generated for all of them, at which the virtual environment aligns itself. A paper about the development of this framework was also published in which more details are described: Link Paper

### Calibration process and [publication](https://zenodo.org/record/4399217#.X-yjaS9XYlI)
The framework uses the Oculus integration for Unity and can be built for the mobile standalone VR glasses Oculus Quest and Quest 2. In order to define the absolute zero point in the room, all users must take turns performing a calibration with their controllers. These must be placed at two points in the room by all of the users. To simplify this process, a [graphic](README_Pictures/QuestCotroller_CenterMark.png) was created that can be printed out on A3 paper and placed on the floor. The calibration can also be applied to other VR glasses that have spatial controller tracking. At the REFRESH#3 conference at the Zurich University of the Arts, an earlier version of this framework was used in the project [Echolocation](https://blog.zhdk.ch/immersivearts/virtual-echolocation/). 

![alt text](README_Pictures/COMultiuserGrafiken-08.png)

### Smartphone camera view
In addition, other viewers can glimpse into the virtual environment via a smartphone. The Unity ARFoundation is used so that they can also move in the space and change their viewing angle. In order for the viewers to be able to move in the same virtual environment relative to the VR users, the smartphone must go through a similar zero-point calibration process. For this, the printable graphic is used as a trackable image. This defines the zero point for the smartphone tracking.

![alt text](README_Pictures/COMultiuserGrafiken-09.png)

## Repository
This Unity project has been developed and tested with Unity 2020.1.12f1.

### Scenes
The two lobby scenes, one for the VR player (IAS lobby) and one for the smartphone player (IAS lobby camera), serve as the local menu for the applications. In these, a host/game can be started and existing hosts or servers can be joined. The "IAS-Server" serves as an example for a passive scene which is not a host and therefore not a client. This should not be run from the Oculus Quest or smartphone. The client scenes serve as game scenes which are automatically started by the network and can be changed in the "MenuUI" object of the respective lobby on the MenuUIManager components. The "NetworkManager" object remains over all scenes (DontDestroyOnLoad).

### Prefabs
#### PauseCalibrationUI
This interface allows the user to start the calibration process. This is opened via the menu button on the left touch controller. A view direction bound courser can be interacted with as the controllers are used for the calibration. 

#### VR Player
This contains the VRRig for the player and the visual body. In a non-local instance of this prefab (player), unneeded components such as the "Camera" or "VRController" are deleted at startup.

Controller gestures and complete finger tracking over the network
Simple Ik body which follows hands and head tracking
BodyScaleManager with which the size can be calibrated via a T-pose in the lobby
The VR controllers can be used to grab objects with the "GrabableNetworkingObject.cs" which adjust their network authority.

#### CameraPlayer
Simplified CameraPlayer on other clients with little logic. Mainly used for position display on other clients.

#### ARCameraManager
Extends the local CameraPlayer with AR functions on the smartphone client.
Image tracking via ARFoundation
A split screen function allows the camera image to be displayed next to the virtual environment.

### Networking
For networking the data, the open-source system [Mirror](https://mirror-networking.com) is used: Link to Mirror In the NetworkManager.cs, in the line 732 change the code from:

	void RegisterClientMessages();
to:

	public virtual void RegisterClientMessages();
	
### [Oculus integration](https://developer.oculus.com/downloads/package/unity-integration/)
In order to enable a full body IK Mesh with the Oculus Quest finger tracking, some changes had to be made to the OVRSkeleton.cs of the Oculus Integration. If you reimport or update the Oculus Integration, you have to change them again.

1.First, add the following code at line 115 in the OVRSkeleton.cs

	public enum BoneOrientations { FromQuatf, FromFlippedXQuatf , FromFlippedZQuatf };
   	public BoneOrientations fingerBoneOrientations = BoneOrientations.FromQuatf;

2. Change the Update function at line 330 to LateUpdate function
3. Change at line 404 the Update(); to LateUpdate();
3. Delete the following Code at line 378:

		_bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
	
4. Add the following Code at line 378:					
	
		if (this.fingerBoneOrientations == BoneOrientations.FromFlippedXQuatf)
		_bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
       	else if (this.fingerBoneOrientations == BoneOrientations.FromFlippedZQuatf)
        	_bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedZQuatf();
       	else
        	_bones[i].Transform.localRotation = data.BoneRotations[i].FromQuatf();
		
5. Add the following code at line 450:

    	public float GetRootScale()
    	{
        	if (!IsInitialized || _dataProvider == null)
            		return 1f;
        	else
            		return _dataProvider.GetSkeletonPoseData().RootScale;
    	}
		
6. Open the LocalVRPlayer prefab, in the child Object HeadsetTrackingCenter, select the LeftHand and change on the OVRCostumeSkeleton Component the FingerBoneOrientation to FromFlippedXQuatf

### Build for Oculus Quest and Android AR Cam
This Unity project can be used to build for the Oculus Quest and for Android smartphones that support ARCore. Two points have to be changed to build for the other devices.

#### Change Build Scenes
For a VR application on the Oculus Quest/2, the "IA Lobby" scene must be the first scene in the Unity build settings. For an ARCore build, the "IA Lobby Camera" scene must be the first scene. The other scene, "IA-Lobby" or "IA-Lobby-Camera", does not need to be built as well.

#### Change XR-Settings
For an Oculus Quest build, Oculus must be active and ARCore inactive in the "XR Plugin Management" settings. For a smartphone build of the camera, the reverse is true. Because of the Oculus Android Manifest, the smartphone app is not shown on the launch screen or in the library. It can be run via Settings/Apps./Show all Apps.

![alt text](README_Pictures/BuildSettings.png)

# Licenses

All text, pictures and models are licensed under  ![CC-BY-NC-SA](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png)

All code is licensed under ![CC-BY](https://i.creativecommons.org/l/by/4.0/88x31.png).

# Credits

Created by MUVR Team at the [IASpace](http://immersive-arts.ch), Zürich University of the Arts, Switzerland.
* Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space
* Oliver Sahli - Associate Researcher at the Immersive Arts Space

![alt text](README_Pictures/EchoLocation_Screenshot1.png)
