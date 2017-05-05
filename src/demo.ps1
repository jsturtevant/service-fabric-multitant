Connect-ServiceFabricCluster

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "src\MyShop\pkg\Debug" -ImageStoreConnectionString "file:C:\SfDevCluster\Data\ImageStoreShare" -ApplicationPackagePathInImageStore "MyShop"

Register-ServiceFabricApplicationType -ApplicationPathInImageStore "MyShop"
