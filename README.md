---

# Install
Install Unity Editor Version 2022.3.14f1. Open the project. Dependencies including Mixed Reality Toolkit and Ros-sharp should automatically install.

# Setup
Requirements: Machine connected to MTM and LapVR, Windows (only tested on windows) laptop / PC with Unity Engine installed, Hololens 2, Ethernet and USB C cables. 
## Wires
Ensure the linux and windows systems are on the same Internet by plugging in the windows laptop to the LCSR ethernet (there should still be a white cable on the tray to the right of the LapVR computer).
Connect the Hololens to the Windows system using its USB C port.
## AMBF Side
Locate the unity_ambf.py file in LapVR Scripts. Change the IP address tstored in the UNITY_IP var to match the ip of the windows system. Unity will determine the IP of the linux system during runtime so no changes needed there. 
The camera offset may need to be adjusted in the unity_ambf.py file. Still unsure why this offset is needed only for the camera. 
## Unity Side
Ensure you are able to use holographic remoting. This is done by installing the Holographic Remoting app on the Hololens and enabling it in Unity.
Ensure UDP Communication is allowed on the windows system. Can be done in Powershell with admin.
## Physical Side
Glue/tape a working QR Code to the top of the Lap VR system. 
```markdown
Set-NetConnectionProfile -InterfaceAlias "Wi-Fi" -NetworkCategory Private
New-NetFirewallRule -DisplayName "Allow UDP 47999" -Direction Inbound -Protocol UDP -LocalPort 47999-48100 -Action Allow -Profile Any

```
# Launch
Launch the AMBF program first. Then run the unity_ambf.py script on the linux machine. Ensure its run first and is always running. Then launch the Holographic Remoting app on Hololens and clikc play on the Unity Editor. While wearing the Hololens register the robot with the QR Code by bringing the headset very close to the QR code for scanning.
Move the robot around with hand rays and when you are satisfied, lift your left hand palm up, and use your right hand to click the "Dock Robot" button that appears near your left hand, locking the robot in place. Use hand rays to manipulate the cannulas wherever you want and then bring the PSM/ECM close to them to mate them. Once all are mated, bring left hand up
and click the begin suture button. This will give control of the PSMs to AMBF.

## Bugs
QR code on remoting has limited support so there is a bug that doesnt allow the same QR code to be scanned in the same remoting session. Each time you need to replay the Unity app you need to completely close the Holographic Remoting App on Hololens and then reopen it. 
