# Get the ASM Library.  This requires you to mount
# a network share so we need a username and password
# --------------------------------------------------
if [ ! -d "./reach/asm" ]; then
  while :
  do
    echo "What is your ADS\***** username? (Enter *****):"
    read username
    sudo mkdir -p /mnt/asm
    sudo mount_smbfs //$username@k2/ASM_Interface /mnt/asm
    mkdir -p ./reach/asm
    sudo rsync -u --progress /mnt/asm/ASM220.3.0.201_rhel6_GCC4.6.3_sdk.tar.gz ./reach/asm/
    [[ $? = 0 ]] && break
  done
  sudo umount /mnt/asm
fi

# Get the LibG Repository and compile it.  
# --------------------------------------------------
if [ ! -d "./reach/libg" ]; then
  sudo mkdir -p ./reach/libg

  sudo git clone -v https://git.autodesk.com/Dynamo/LibG ./reach/libg
fi

# Securely copy ASM files over to the other side.
# --------------------------------------------------
echo "What is the AWS IP to send to?"
read awsaddress
scp ./remote.sh ubuntu@$awsaddress:~
scp -r ./reach/asm ubuntu@$awsaddress:~
scp -r ./reach/libg ubuntu@$awsaddress:~

ssh ubuntu@$awsaddress
