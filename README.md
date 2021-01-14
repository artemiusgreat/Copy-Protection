# Copy protection

Copy protection tool helps to protect your app from copying to another computer. 
Useful, when you sell your app and don't want the customer to share it with anyone for free. 

# The workflow

* Launch the app.
* The app checks if there is HD serial number in the file started the process.
* If file doesn't have a serial number, copy the source of EXE file, add current serial number, restart the process, remove original EXE file.
* On every next launch the app will try to compare current HD serial number with the one in the file. If EXE file was copied to computer with a different serial number, the app will throw an exception. 

# Usage 

Add this project to your solution and add reference to it. 
Whenever you want to check in yur code that your app is running on the same hard drive, call this method. 
Done. 

```
Protector.CheckDrive();
```