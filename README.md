# Copy protection

Copy protection tool helps to protect your app from copying to another computer. 
Useful, when you sell your app and don't want the customer to share it with anyone for free. 

# Workflow

* Launch the app.
* The app checks if there is HD serial number in the file started the process.
* If file doesn't have a serial number, modify EXE file to add current serial number, restart the process, remove original EXE.
* On every next launch the app will try to compare current HD serial number with the one in the file. 
* If the serial number in EXE file doesn't match with the one on the current computer, the app will throw an exception. 

# Usage 

Add this project to your solution and add reference to it. 
Whenever you want to check in yur code that your app is running on the same hard drive, call this method. 
Done. 

```
Protector.CheckDrive();
```
