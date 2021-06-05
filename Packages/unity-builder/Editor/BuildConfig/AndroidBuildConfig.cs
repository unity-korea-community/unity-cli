using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace UNKO.Unity_Builder
{
    /// <summary>
    /// 안드로이드 빌드 설정
    /// </summary>
    [CreateAssetMenu(fileName = "AndroidBuildConfig", menuName = GlobalConst.CreateAssetMenu_Prefix + "/AndroidBuildConfig")]
    [StructLayout(LayoutKind.Auto)] // ignore codacy
    public class AndroidBuildConfig : BuildConfigBase
    {
        ///<inheritdoc cref="IBuildConfig.buildTarget"/>
        public override BuildTarget buildTarget => BuildTarget.Android;

        /// <summary>
        /// 안드로이드 빌드에 사용할 keyaliasName
        /// </summary>
        [SerializeField]
        protected string keyaliasName;

        /// <summary>
        /// 안드로이드 빌드에 사용할 keyaliasPassword
        /// </summary>
        [SerializeField]
        protected string keyaliasPassword;

        /// <summary>
        /// Keystore 파일의 경로입니다. `파일경로/파일명.keystore` 까지 쓰셔야 합니다.
        /// <para>UnityProject/Asset/ 기준의 상대 경로입니다. </para>
        /// <para>예를들어 UnityProject/Asset 폴더 밑에 example.keystore가 있으면 "/example.keystore" 입니다.</para>
        /// </summary>
        [SerializeField]
        protected string keystorePath;

        /// <summary>
        /// 안드로이드 keystore password
        /// </summary>
        [SerializeField]
        protected string keystorePassword;

        /// <summary>
        /// CPP 빌드를 할지 체크, CPP빌드는 오래 걸리므로 Test빌드가 아닌 Alpha 빌드부터 하는걸 권장
        /// </summary>
        [SerializeField]
        protected ScriptingImplementation scriptingBackEnd;

        /// <summary>
        /// 안드로이드 빌드에 사용할 bundleVersionCode
        /// </summary>
        [SerializeField]
        protected int bundleVersionCode;

        ///<inheritdoc cref="IBuildConfig.ResetSetting"/>
        public override void ResetSetting(BuildConfigBase config)
        {
            base.ResetSetting(config);

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            scriptingBackEnd = PlayerSettings.GetScriptingBackend(targetGroup);
            bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            config.AddBuildPath(
                "\n_{bundleVersion}.{bundleVersionCode}" +
                "\n_{scriptingBackEnd}");
        }

        ///<inheritdoc cref="IBuildConfig.OnPreBuild"/>
        public override void OnPreBuild(IDictionary<string, string> commandLine, ref BuildPlayerOptions buildPlayerOptions)
        {
            base.OnPreBuild(commandLine, ref buildPlayerOptions);

            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keyaliasPass = keyaliasPassword;

            PlayerSettings.Android.keystoreName = Application.dataPath + keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scriptingBackEnd);

            Debug.LogFormat($"OnPreBuild [Android]\n" +
                            $"PackageName : {PlayerSettings.applicationIdentifier}\n" +
                            $"keyaliasName : {PlayerSettings.Android.keyaliasName}, keyaliasPass : {PlayerSettings.Android.keyaliasPass}\n" +
                            $"keystoreName : {PlayerSettings.Android.keystoreName}, keystorePass : {PlayerSettings.Android.keystorePass}\n");
        }

        ///<inheritdoc cref="IBuildConfig.GetBuildPath"/>
        public override string GetBuildPath()
        {
            return base.GetBuildPath()
                .Replace("{scriptingBackEnd}", scriptingBackEnd.ToString())
                .Replace("{bundleVersionCode}", bundleVersionCode.ToString())
                + ".apk";
        }
    }

    /// <summary>
    /// <see cref="AndroidBuildConfig"/> 인스펙터
    /// </summary>
    [CustomEditor(typeof(AndroidBuildConfig))]
    public class AndroidBuildConfig_Inspector : Editor
    {
        string _commandLine;

        ///<inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            AndroidBuildConfig config = target as AndroidBuildConfig;
            if (GUILayout.Button("Reset to Current EditorSetting"))
            {
                config.ResetSetting(config);
            }

            if (GUILayout.Button("Build!"))
            {
                UnityBuilder.Build(config);
            }

            _commandLine = EditorGUILayout.TextField("commandLine", _commandLine);
            if (GUILayout.Button($"Build with \'{_commandLine}\'"))
            {
                string[] commands = _commandLine.Split(' ');
                for (int i = 0; i < commands.Length; i++)
                {
                    string command = commands[i];
                    bool hasNextCommand = i + 1 < commands.Length;
                    if (command.StartsWith("-"))
                    {
                        if (hasNextCommand)
                        {
                            Environment.SetEnvironmentVariable(command, commands[i + 1]);
                        }
                        else
                        {
                            Environment.SetEnvironmentVariable(command, "");
                        }
                    }
                }

                UnityBuilder.Build();
            }
        }
    }
}
