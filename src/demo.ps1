Connect-ServiceFabricCluster

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "C:\gitvturecek\job-creator-demo\src\JobCreatorDemo\pkg\Debug" -ImageStoreConnectionString "file:C:\SfDevCluster\Data\ImageStoreShare" -ApplicationPackagePathInImageStore "JobCreatorDemo"

Register-ServiceFabricApplicationType -ApplicationPathInImageStore "JobCreatorDemo"

New-ServiceFabricApplication -ApplicationName "fabric:/Demo2" -ApplicationTypeName "JobCreatorDemoType" -ApplicationTypeVersion "1.0.0" -ApplicationParameter @{"WebService_AppPath" = "demo2"; }