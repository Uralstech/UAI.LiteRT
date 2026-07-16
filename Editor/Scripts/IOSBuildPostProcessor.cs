// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_IOS

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

#nullable enable
namespace Uralstech.UAI.LiteRTLM.Editor
{
    internal sealed class IOSBuildPostProcessor : IPostprocessBuildWithReport
    {
        private const string DeviceLibPath = "$(PROJECT_DIR)/Libraries/ARM64/Packages/com.uralstech.uai.litertlm/Runtime/Plugins/iOS/arm64";
        private const string SimLibPath = "$(PROJECT_DIR)/Libraries/ARM64Simulator/Packages/com.uralstech.uai.litertlm/Runtime/Plugins/iOS/sim_arm64";
        
        public int callbackOrder => 0;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS
                || report.summary.buildType != BuildType.Player
                || report.summary.result is BuildResult.Failed or BuildResult.Cancelled
                || PlayerSettings.iOS.simulatorSdkArchitecture == AppleMobileArchitectureSimulator.X86_64)
                return;
            
            string projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            
            PBXProject project = new();
            project.ReadFromFile(projectPath);
            
            string libPath = PlayerSettings.iOS.sdkVersion == iOSSdkVersion.DeviceSDK
                ? DeviceLibPath : SimLibPath;
            
            string mainTargetGuid = project.GetUnityFrameworkTargetGuid();
            project.AddBuildProperty(mainTargetGuid, "LIBRARY_SEARCH_PATHS", libPath);
            
            project.WriteToFile(projectPath);
        }
    }
}

#endif