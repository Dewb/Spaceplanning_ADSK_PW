# Ensure that external shared library dependecies are installed.
# -------------------------------------------------
sudo apt-get update
sudo apt-get upgrade -y
sudo apt-get install -y git
sudo apt-get install -y cifs-utils
sudo apt-get install -y aptitude
sudo apt-get install -y mono-complete
sudo aptitude install -y swig
sudo aptitude install -y libboost1.55-dev
sudo aptitude install libboost-thread1.55-dev
sudo aptitude install -y build-essential

curl -sL https://deb.nodesource.com/setup | sudo bash -
sudo apt-get install -y nodejs

cd ~

echo 'Copying ASM into code/asm'
# Move the ASM files into the code directory.
# --------------------------------------------------
if [ ! -d "./code/asm" ]; then
  mkdir -p ./code/asm
  mv ~/asm/* ./code/asm
  rmdir ~/asm
fi



# Move the LibG file to the code directory, open and compile it.
# -------------------------------------------------
if [ ! -d "./code/libg/asm/asm_sdk_220" ]; then
  echo 'Moving libg to code/libg'
  mkdir -p ./code/libg
  mv ~/libg/* ./code/libg
  sudo rm -r ~/libg

  mkdir -p ./code/libg/asm/asm_sdk_220
  echo 'Opening ASM into libg/asm/asm_sdk_220'
  tar -xvf ./code/asm/ASM220.3.0.201_rhel6_GCC4.6.3_sdk.tar.gz -C ./code/libg/asm/asm_sdk_220/

  pushd ./code/libg
  echo 'Making libg'
  make
  popd
fi

# Get the Dynamo Repository and compile it.
# --------------------------------------------------
if [ ! -d "./code/dynamo/.git" ]; then
  mkdir -p ./code/dynamo

  echo 'Git cloning Dynamo'
  git clone -v https://github.com/DynamoDS/Dynamo ./code/dynamo

  pushd ./code/dynamo/src
  echo 'Building Dynamo'
  sudo xbuild Dynamo.Mono.2013.sln
  popd

  #export DYNAMOAPI=$(readlink -f ./code/Dynamo/bin/AnyCPU/Release)
fi

# Get the Reach Repository and compile it.
# Contact Peter Boyer for access to Github repo.
# --------------------------------------------------
if [ ! -d "./code/reach/.git" ]; then
  mkdir -p ./code/reach

  # We need Dynamo in this location today for Reach...
  pushd ./code
  echo 'Linking dynamo and Dynamo'
  ln -s ./dynamo ./Dynamo
  popd

  git clone -v https://github.com/DynamoDS/Reach ./code/reach

  pushd ./code/reach/src
  sudo xbuild Reach.Mono.sln
  popd

  pushd ./code/dynamo/bin/AnyCPU/Debug
  sed -i 's/220/219/g' ProtoGeometry.config
  popd
fi


## Run Reach
export MONO_LOG_LEVEL=debug
sudo mono ./code/dynamo/bin/AnyCPU/Debug/Reach.Rest.exe