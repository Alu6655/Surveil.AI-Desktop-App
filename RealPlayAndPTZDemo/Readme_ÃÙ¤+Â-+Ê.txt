﻿[Introduction]
The demo program introduces SDK initialization, login, logout, auto reconnection, start monitoring, stop monitoring, snapshot, enable saving stream, disable saving stream, PTZ control.
The demo program demonstrates selecting channel and stream type before live view, PTZ control ( including direction control), and the function of changing step, zoom, focus and iris when the device is connected. 

[Interfaces]
NETClient.Init Initialize SDK and set disconnection callback
NETClient.SetAutoReconnect Set auto reconnection callback
NETClient.SetDVRMessCallBack Set snapshot callback
NETClient.LoginWithHighLevelSecurity Login
NETClient.Logout  Logout
NETClient.RealPlay Start real time monitor
NETClient.SetRealDataCallBack Set real time monitoring data callback
NETClient.StopRealPlay Stop monitoring
NETClient.SnapPictureEx Snapshot
NETClient.SnapPictureEx  Local snapshot
NETClient.SaveRealData Save monitoring data
NETClient.StopSaveRealData Stop saving  monitoring data
NETClient.PTZControl PTZ control
NETClient.Cleanup Release SDK resources

[Notice]
1. When the compiling environment is VS2010, NETSDKCS library can support the version of .NET Framework 4.0 or newer. If you want to use the version older than .NET Framework 4.0, change the method of using NetSDK.cs in IntPtr.Add. We will not support the modification.
2. The demo program supports  single channel and single device live view.
3. The demo program does not support multiple devices login.
4. Issue: No response to snapshot. Cause: The device does not support. For example: intelligent traffic device has special snapshot interface, but it does not support common snapshot; Whether NVR can snapshot depending on whether the connected IPC supports snapshot. 
5. Start saving. If the saved record is in Dahua video format, it can be only played by Dahua playing SDK or Dahua player.
6. Copy all file in the libs directory of General_NetSDK_ChnEng_CSharpXX_IS_VX.XXX.XXXXXXXX.X.R.XXXXXX to the build directory of bin directory of the corresponding demo programs.
7. Copy all file in the libs directory of General_NetSDK_ChnEng_CSharpXX_IS_VX.XXX.XXXXXXXX.X.R.XXXXXX to the build directory of bin directory of the corresponding demo programs.

【演示程序功能】
1、演示程序介绍了SDK初始化、登陆设备、登出设备、自动重连、打开监视预览、关闭监视、抓图、保存码流、关闭保存码流、PTZ控制功能。
2、演示程序演示了在预览前可选择通道、码流类型，在窗口标题有设备连接状态，PTZ控制包括方向控制，可更改步长，缩放、焦距、光圈等部分功能。

【接口列表】
NETClient.Init SDK初始化与设置断线回调
NETClient.SetAutoReconnect 设置自动重连回调
NETClient.SetSnapRevCallBack 设置抓图回调
NETClient.LoginWithHighLevelSecurity 登录设备
NETClient.Logout 登录
NETClient.RealPlay 实时监视
NETClient.SetRealDataCallBack 设置实时监视数据回调
NETClient.StopRealPlay 停止监视
NETClient.SnapPictureEx 远程抓图
NETClient.CapturePicture 本地抓图
NETClient.SaveRealData 保存监视数据
NETClient.StopSaveRealData 停止保存数据
NETClient.PTZControl 云台控制
NETClient.Cleanup 释放SDK资源

【注意事项】
1、编译环境为VS2010，NETSDKCS库最低只支持.NET Framework 4.0,如用户需要支持低于4.0的版本需要更改NetSDK.cs文件中使用到IntPtr.Add的方法,我们不提供修改。
2、此演示程序只支持单设备单通道预览。
3、此演示程序不支持多设备登陆。
4、抓图没响应问题，可能设备不支持，如：智能交通设备有专用的抓图接口，不支持普通抓图;NVR需要正确配置才能抓图，取决于NVR链接的IPC是否支持抓图。
5、开始保存，保存的录像是大华视频格式录像，只能用大华播放SDK或播放器播发。
6、请把General_NetSDK_ChnEng_CSharpXX_IS_VX.XXX.XXXXXXXX.X.R.XXXXXX中libs目录中的所有文件复制到对应该演示程序中bin目录下的生成目录中。

