To run this setup:

Clone this directory. Make sure both setup.sh and remote.sh files are in the same directory.

- You must be connected to the the internet and to the Autodesk network.
From OS X, a way to do so is to go Finder > Go > Connect to Server... > smb://k2/ASM_Interface
While connecting, you will be prompted for your ADS username and password.

- You must have:
• Root access to the current computer
• SSH privileges set up to the AWS Linux Ubuntu environment.
• Access to the Reach repository on Github. Contact Peter Boyer to get access.
• Git installed in your current machine.

- Stay nearby! You will be prompted for your ADS and github usernames and passwords, as well as the AWS machine's IP.


When setting up the Amazon side, your console may stop and be propmted that there is a new version /boot/grub/menu.lst. This is part of the Amazon EC2 set-up, just choose "keep the local version currently installed".

Usage:
```$sudo sh setup.sh```

You will be SSHed into the machine. Once in it, do:
```$sudo /bin/bash ~/remote.sh```

When prompted, input the Github username and password.