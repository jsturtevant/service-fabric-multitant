Connect-ServiceFabricCluster

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "C:\Projects\sf\job-creator-demo\src\MyShop\pkg\Debug" -ImageStoreConnectionString "file:C:\SfDevCluster\Data\ImageStoreShare" -ApplicationPackagePathInImageStore "MyShop"

Register-ServiceFabricApplicationType -ApplicationPathInImageStore "MyShop"
