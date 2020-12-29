If you import or update the Oculus Integration, you have to change some small things on the OVRSkeleton.cs script to work with one singel full body model
1.First, add the following code at line 115 in the OVRSkeleton.cs

    public enum BoneOrientations { FromQuatf, FromFlippedXQuatf , FromFlippedZQuatf };
    public BoneOrientations fingerBoneOrientations = BoneOrientations.FromQuatf;

2. Change the Update function at line 330 to LateUpdate function
3. Change at line 404 the Update(); to LateUpdate();
3. Delet at line 378 the following Code: _bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
4. Add at 378 the following Code
					if (this.fingerBoneOrientations == BoneOrientations.FromFlippedXQuatf)
                        _bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
                    else if (this.fingerBoneOrientations == BoneOrientations.FromFlippedZQuatf)
                        _bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedZQuatf();
                    else
                        _bones[i].Transform.localRotation = data.BoneRotations[i].FromQuatf();

5. Add the following code at 450
    public float GetRootScale()
    {
        if (!IsInitialized || _dataProvider == null)
            return 1f;
        else
            return _dataProvider.GetSkeletonPoseData().RootScale;
    }
		
6. Open the LocalVRPlayer prefab, in the child Object HeadsetTrackingCenter, select the LeftHand and change on the OVRCostumeSkeleton Component the FingerBoneOrientation to FromFlippedXQuatf